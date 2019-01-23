namespace Network
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using UnityEngine;


    public class Server
    {
        /// <summary> 	
        /// TCPListener to listen for incoming TCP connection 	
        /// requests. 	
        /// </summary> 	
        private TcpListener tcpListener;

        /// <summary> 
        /// Background thread for TcpServer workload. 	
        /// </summary> 	
        private Thread tcpListenerThread;

        private readonly List<Client> clients = new List<Client>();

        private bool stopped;
        private IPAddress address;
        private int port;

        private TimeSpan clientConnectionCheckTimeout = TimeSpan.FromSeconds(30);
        private byte nextClientId = 0;

        private byte[] writeBuffer = new Byte[1024];

        public event MessageReceivedDelegate MessageReceived;

        public delegate void MessageReceivedDelegate(MessageHeader header, byte[] buffer);

        public Server() : this(IPAddress.Parse("127.0.0.1"), 8052)
        {
        }

        public Server(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;
        }

        public void Start()
        {
            try
            {
                // Start TcpServer background thread
                this.tcpListenerThread = new Thread(this.ListenForData) { IsBackground = true };
                this.tcpListenerThread.Start();
            }
            catch (Exception exception)
            {
                Debug.Log("On client connect exception " + exception);
            }
        }

        public void Stop()
        {
            if (this.stopped)
            {
                Debug.LogWarning("[Client] Server already stopped.");
                return;
            }

            this.stopped = true;
            this.tcpListener.Stop();

            while (this.tcpListenerThread.IsAlive)
            {
                Thread.Sleep(10);
            }

            Debug.Log("[Client] Server stopped correctly.");
        }

        /// <summary> 	
        /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
        /// </summary> 	
        private void ListenForData()
        {
            try
            {
                Byte[] readBuffer = new Byte[1024];

                this.tcpListener = new TcpListener(this.address, this.port);
                this.tcpListener.Start();
                Debug.Log($"[Server] Server is listening on {this.address} port {this.port}.");

                while (!this.stopped)
                {
                    if (this.tcpListener.Pending())
                    {
                        // New incoming client.
                        var newClient = this.tcpListener.AcceptTcpClient();
                        var client = new Client(this.nextClientId, newClient);
                        this.clients.Add(client);
                        this.nextClientId++;
                        Debug.Log($"[Server] New client connected ({client}). {this.clients.Count} client(s) now connected.");
                    }

                    for (var clientIndex = this.clients.Count - 1; clientIndex >= 0; clientIndex--)
                    {
                        var client = this.clients[clientIndex];

                        if (DateTime.Now - client.LastMessageTime > this.clientConnectionCheckTimeout)
                        {
                            this.SendMessage(client, Message.Ping());
                        }

                        if (!client.TcpClient.Connected)
                        {
                            client.Stream.Close();
                            client.TcpClient.Close();
                            this.clients.RemoveAt(clientIndex);
                            Debug.Log($"[Server] Client disconnected. {this.clients.Count} client(s) remain connected.");
                            continue;
                        }

                        while (client.TcpClient.Available > 0)
                        {
                            client.LastMessageTime = DateTime.Now;
                            client.Stream.Read(readBuffer, 0, readBuffer.Length);

                            var header = MessageHeader.Parse(readBuffer);
                            Debug.Log($"[Server] Message received from client {client}: {header}");
                            if (header.Type == MessageType.Ping)
                            {
                                this.SendMessage(client, Message.Pong());
                            }

                            this.MessageReceived?.Invoke(header, readBuffer);
                        }

                        this.clients[clientIndex] = client;
                    }

                    Thread.Sleep(100);
                }
            }
            catch (SocketException socketException)
            {
                Debug.LogError("[Server] SocketException " + socketException.ToString());
            }
            catch (Exception exception)
            {
                Debug.LogError("[Server] Exception: " + exception);
            }
        }
        
        public void BroadcastAll(Message message)
        {
            if (this.tcpListener == null)
            {
                return;
            }

            foreach (var client in this.clients)
            {
                this.SendMessage(client, message);
            }
        }

        /// <summary> 	
        /// Send message to client using socket connection. 	
        /// </summary> 	
        private void SendMessage(Client client, Message message)
        {
            if (this.tcpListener == null || !client.Stream.CanWrite)
            {
                return;
            }

            try
            {
                message.Write(this.writeBuffer);
                client.Stream.Write(this.writeBuffer, 0, this.writeBuffer.Length);
            }
            catch (Exception socketException)
            {
                Debug.Log("[Server] Socket exception: " + socketException);
            }
        }

        public struct Client
        {
            public int Id;
            public TcpClient TcpClient;
            public NetworkStream Stream;
            public DateTime LastMessageTime;

            public Client(int id, TcpClient newClient)
            {
                this.Id = id;
                this.TcpClient = newClient;
                this.Stream = newClient.GetStream();
                this.LastMessageTime = DateTime.Now;
            }

            public override string ToString()
            {
                return this.Id.ToString();
            }
        }
    }
}

