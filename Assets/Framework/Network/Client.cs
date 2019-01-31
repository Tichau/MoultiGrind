using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Framework.Network
{
    public class Client : IDisposable
    {
        public byte Id = Server.InvalidClientId;

        private readonly string hostname;
        private readonly int port;

        private InterfaceState state;

        private MemoryStream writeStream;
        private BinaryWriter writer;

        private TcpClient tcpClient;
        private Thread clientReceiveThread;
        private NetworkStream networkStream;

        public event MessageReceivedDelegate MessageReceived;

        public delegate void MessageReceivedDelegate(MessageHeader header, BinaryReader buffer);

        public Client() : this("localhost", 8052)
        {
        }

        public Client(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }

        public InterfaceState State => this.state;

        public void Start()
        {
            if (this.state != InterfaceState.None)
            {
                Debug.LogWarning($"[{this}] Client has already been started once.");
                return;
            }

            try
            {
                this.writeStream = new MemoryStream();
                this.writer = new BinaryWriter(this.writeStream);
                this.clientReceiveThread = new Thread(this.ListenForData) { IsBackground = true };
                this.clientReceiveThread.Start();
                this.state = InterfaceState.Started;
            }
            catch (Exception exception)
            {
                Debug.Log("[{this}] On client connect exception " + exception);
            }
        }

        public void Stop()
        {
            if (this.state != InterfaceState.Started)
            {
                Debug.LogWarning($"[{this}] Client already stopped.");
                return;
            }

            this.state = InterfaceState.Stopped;
            this.tcpClient.Close();

            while (this.clientReceiveThread.IsAlive)
            {
                Thread.Sleep(10);
            }

            this.networkStream.Close();
            this.networkStream.Dispose();
            this.tcpClient.Dispose();

            this.writeStream.Dispose();
            this.writer.Dispose();

            Debug.Log($"[{this}] Client stopped correctly ");
        }

        public void SendMessage(Stream message)
        {
            if (this.networkStream == null || !this.networkStream.CanWrite)
            {
                return;
            }

            try
            {
                Debug.Log($"[{this}] send message.");
                message.Seek(0, SeekOrigin.Begin);
                message.CopyTo(this.networkStream);
            }
            catch (Exception socketException)
            {
                Debug.Log($"[{this}] Socket exception: " + socketException);
            }
        }

        private void SendMessage(Message message)
        {
            if (this.networkStream == null || !this.networkStream.CanWrite)
            {
                return;
            }

            try
            {
                this.writer.BaseStream.Seek(0, SeekOrigin.Begin);
                this.writer.WriteMessage(message);
                this.writer.BaseStream.Seek(0, SeekOrigin.Begin);
                this.writeStream.CopyTo(this.networkStream);
            }
            catch (Exception socketException)
            {
                Debug.Log($"[{this}] Socket exception: " + socketException);
            }
        }

        private void ListenForData()
        {
            try
            {
                byte[] readBuffer = new byte[4096];
                using (MemoryStream readStream = new MemoryStream(readBuffer))
                using (BinaryReader reader = new BinaryReader(readStream))
                {
                    this.tcpClient = new TcpClient(this.hostname, this.port);
                    this.networkStream = tcpClient.GetStream();
                    Debug.Log($"[{this}] Client connected to server {this.hostname} port {this.port}.");
                    
                    while (this.state != InterfaceState.Stopped)
                    {
                        if (this.tcpClient.Available <= 0)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        this.networkStream.Read(readBuffer, 0, readBuffer.Length);

                        readStream.Seek(0, SeekOrigin.Begin);
                        var header = reader.ReadHeader();

                        if (header.Type == MessageType.Ping)
                        {
                            Debug.Log($"[{this}] Ping received from server.");
                            this.SendMessage(Message.Pong);
                        }
                        else if (header.Type == MessageType.Connect)
                        {
                            reader.ReadConnectMessage(out this.Id);
                            Debug.Log($"[{this}] Client identified by server with id {this.Id}.");
                        }
                        else
                        {
                            Debug.Assert(this.Id != Server.InvalidClientId, $"[{this}] Client is not identified by server.");
                        }

                        try
                        {
                            this.MessageReceived?.Invoke(header, reader);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogError($"[{this}] Exception thrown on message received callback.\n{exception}");
                        }
                    }

                    Debug.Log($"[{this}] Socket closed. Stop the client.");
                    this.Stop();
                }
            }
            catch (IOException ioException)
            {
                if (this.state == InterfaceState.Stopped)
                {
                    // This exception happen when we close the socket while trying to stop the client.
                    Debug.Log($"[{this}] IO exception: " + ioException);
                }
                else
                {
                    Debug.LogError($"[{this}] IO exception: " + ioException);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{this}] Exception: " + exception);
            }
        }

        public void Dispose()
        {
            this.Stop();
        }

        public override string ToString()
        {
            if (this.Id == Server.InvalidClientId)
            {
                return "Client~";
            }

            return $"Client{this.Id}";
        }
    }
}