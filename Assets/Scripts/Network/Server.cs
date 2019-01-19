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

        private List<Client> clients = new List<Client>();

        private bool stopped;
        private IPAddress address;
        private int port;

        private byte[] writeBuffer = new Byte[1024];
        private TimeSpan clientConnectionCheckTimeout = TimeSpan.FromSeconds(30);

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
                        this.clients.Add(new Client(newClient));
                        Debug.Log($"[Server] New client connected. {this.clients.Count} client(s) now connected.");
                    }

                    for (var index = this.clients.Count - 1; index >= 0; index--)
                    {
                        var client = this.clients[index];

                        if (DateTime.Now - client.LastMessageTime > this.clientConnectionCheckTimeout)
                        {
                            this.SendMessage(client, Message.Ping());
                        }

                        if (!client.TcpClient.Connected)
                        {
                            client.Stream.Close();
                            client.TcpClient.Close();
                            this.clients.RemoveAt(index);
                            Debug.Log($"[Server] Client disconnected. {this.clients.Count} client(s) remain connected.");
                            continue;
                        }

                        while (client.TcpClient.Available > 0)
                        {
                            client.LastMessageTime = DateTime.Now;
                            client.Stream.Read(readBuffer, 0, readBuffer.Length);
                            var message = Message.Parse(readBuffer);
                            Debug.Log($"[Server] Message received from client {index}: {message}");

                            if (message.Type == MessageType.Ping)
                            {
                                this.SendMessage(client, Message.Pong());
                            }
                        }

                        this.clients[index] = client;
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
            public TcpClient TcpClient;
            public NetworkStream Stream;
            public DateTime LastMessageTime;

            public Client(TcpClient newClient)
            {
                this.TcpClient = newClient;
                this.Stream = newClient.GetStream();
                this.LastMessageTime = DateTime.Now;
            }
        }
    }
}

