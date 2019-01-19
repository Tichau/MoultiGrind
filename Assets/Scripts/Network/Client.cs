namespace Network
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using UnityEngine;

    public class Client
    {
        private string hostname;
        private int port;
        private bool stopped;

        private TcpClient socketConnection;
        private Thread clientReceiveThread;
        private NetworkStream networkStream;
        private byte[] writeBuffer = new byte[1024];

        public Client() : this("localhost", 8052)
        {
        }

        public Client(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;
        }
        
        /// <summary> 	
        /// Send message to client using socket connection. 	
        /// </summary> 	
        public void SendMessage(Message message)
        {
            if (this.networkStream == null || !this.networkStream.CanWrite)
            {
                return;
            }

            try
            {
                message.Write(this.writeBuffer);
                this.networkStream.Write(this.writeBuffer, 0, this.writeBuffer.Length);
            }
            catch (Exception socketException)
            {
                Debug.Log("[Server] Socket exception: " + socketException);
            }
        }

        public void Start()
        {
            try
            {
                this.clientReceiveThread = new Thread(this.ListenForData) { IsBackground = true };
                this.clientReceiveThread.Start();
            }
            catch (Exception exception)
            {
                Debug.Log("[Client] On client connect exception " + exception);
            }
        }

        public void Stop()
        {
            if (this.stopped)
            {
                Debug.LogWarning("[Client] Client already stopped.");
                return;
            }

            this.stopped = true;
            this.socketConnection.Close();

            while (this.clientReceiveThread.IsAlive)
            {
                Thread.Sleep(10);
            }

            this.networkStream.Close();
            this.networkStream.Dispose();
            this.socketConnection.Dispose();

            Debug.Log("[Client] Client stopped correctly ");

            this.networkStream = null;
            this.socketConnection = null;
            this.clientReceiveThread = null;
        }

        /// <summary>
        /// Listens for incomming data.
        /// </summary>
        private void ListenForData()
        {
            try
            {
                Byte[] readBuffer = new Byte[1024];

                this.socketConnection = new TcpClient(this.hostname, this.port);
                this.networkStream = socketConnection.GetStream();
                Debug.Log($"[Client] Client connected to server {this.hostname} port {this.port}.");

                int length;
                while ((length = this.networkStream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                {
                    var message = Message.Parse(readBuffer);
                    Debug.Log($"[Client] Message received from server: {message}");

                    if (message.Type == MessageType.Ping)
                    {
                        this.SendMessage(Message.Pong());
                    }
                }

                Debug.Log("[Client] Socket closed. Stop the client.");
                this.Stop();
            }
            catch (System.IO.IOException ioException)
            {
                if (this.stopped)
                {
                    // This exeption happen when we close the socket while trying to stop the client.
                    Debug.Log("[Client] IO exception: " + ioException);
                }
                else
                {
                    Debug.LogError("[Client] IO exception: " + ioException);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError("[Client] Exception: " + exception);
            }
        }
    }
}