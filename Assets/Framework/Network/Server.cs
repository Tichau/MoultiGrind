﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Framework.Network
{
    public class Server : IDisposable
    {
        public const byte InvalidClientId = byte.MaxValue;

        /// <summary> 	
        /// TCPListener to listen for incoming TCP connection 	
        /// requests. 	
        /// </summary> 	
        private TcpListener tcpListener;

        /// <summary> 
        /// Background thread for TcpServer workload. 	
        /// </summary> 	
        private Thread tcpListenerThread;

        private readonly TimeSpan clientConnectionCheckTimeout;

        private readonly List<Client> clients = new List<Client>();

        private readonly MemoryStream writeStream = new MemoryStream();
        private readonly BinaryWriter writer;

        private IPAddress address;
        private int port;

        private InterfaceState state;

        private byte nextClientId = 0;

        public event MessageReceivedDelegate MessageReceived;

        public delegate void MessageReceivedDelegate(byte clientId, MessageHeader header, BinaryReader buffer);

        public Server() : this(IPAddress.Parse("127.0.0.1"), 8052, TimeSpan.FromSeconds(30))
        {
        }

        public Server(IPAddress address, int port, TimeSpan clientConnectionCheckTimeout)
        {
            this.address = address;
            this.port = port;
            this.writer = new BinaryWriter(this.writeStream);
            this.clientConnectionCheckTimeout = clientConnectionCheckTimeout;
        }

        public int ClientCount => this.clients.Count;

        public InterfaceState State => this.state;

        public void Start()
        {
            if (this.state != InterfaceState.None)
            {
                Debug.LogWarning($"[Server] Server has already been started once.");
                return;
            }

            try
            {
                // Start TcpServer background thread
                this.tcpListenerThread = new Thread(this.ListenForData) { IsBackground = true };
                this.tcpListenerThread.Start();
                this.state = InterfaceState.Started;
            }
            catch (Exception exception)
            {
                Debug.Log("On client connect exception " + exception);
            }
        }

        public void Stop()
        {
            if (this.state != InterfaceState.Started)
            {
                Debug.LogWarning("[Server] Server already stopped.");
                return;
            }

            this.state = InterfaceState.Stopped;
            this.tcpListener.Stop();

            while (this.tcpListenerThread.IsAlive)
            {
                Thread.Sleep(10);
            }

            this.writeStream.Dispose();
            this.writer.Dispose();

            Debug.Log("[Server] Server stopped correctly.");
        }

        public void SendMessage(byte clientId, Stream message)
        {
            int clientIndex = -1;
            for (int index = 0; index < this.clients.Count; index++)
            {
                if (this.clients[index].Id == clientId)
                {
                    clientIndex = index;
                }
            }

            if (this.tcpListener == null || clientIndex < 0)
            {
                return;
            }

            Client client = this.clients[clientIndex];

            if (!client.Stream.CanWrite)
            {
                return;
            }

            try
            {
                message.Seek(0, SeekOrigin.Begin);
                message.CopyTo(client.Stream);
            }
            catch (Exception socketException)
            {
                Debug.Log("[Server] Socket exception: " + socketException);
            }
        }

        public void SendMessage(Client client, Stream message)
        {
            if (this.tcpListener == null || !client.Stream.CanWrite)
            {
                return;
            }

            try
            {
                message.Seek(0, SeekOrigin.Begin);
                message.CopyTo(client.Stream);
            }
            catch (Exception socketException)
            {
                Debug.Log("[Server] Socket exception: " + socketException);
            }
        }

        /// <summary> 	
        /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
        /// </summary> 	
        private void ListenForData()
        {
            try
            {
                byte[] readBuffer = new byte[4096];
                using (MemoryStream readStream = new MemoryStream(readBuffer))
                using (BinaryReader reader = new BinaryReader(readStream))
                {
                    this.tcpListener = new TcpListener(this.address, this.port);
                    this.tcpListener.Start();
                    Debug.Log($"[Server] Server is listening on {this.address} port {this.port}.");

                    while (this.state != InterfaceState.Stopped)
                    {
                        if (this.tcpListener.Pending())
                        {
                            // New incoming client.
                            var newClient = this.tcpListener.AcceptTcpClient();
                            Debug.Assert(this.nextClientId != InvalidClientId);
                            var client = new Client(this.nextClientId, newClient);
                            this.clients.Add(client);
                            this.nextClientId++;
                            Debug.Log($"[Server] New client connected ({client}). {this.ClientCount} client(s) now connected.");

                            this.writer.WriteConnectMessage(client.Id);
                            this.SendMessage(client, this.writeStream);
                        }

                        for (var clientIndex = this.clients.Count - 1; clientIndex >= 0; clientIndex--)
                        {
                            var client = this.clients[clientIndex];

                            if (DateTime.Now - client.LastMessageTime > this.clientConnectionCheckTimeout)
                            {
                                Debug.Log($"[Server] Send Ping to client {client.Id}.");
                                this.writeStream.Position = 0;
                                this.writer.WriteHeader(new MessageHeader(0, MessageType.Ping));
                                this.SendMessage(client, this.writeStream);
                            }

                            if (!client.TcpClient.Connected)
                            {
                                client.Stream.Close();
                                client.TcpClient.Close();
                                this.clients.RemoveAt(clientIndex);
                                Debug.Log($"[Server] Client disconnected. {this.ClientCount} client(s) remain connected.");
                                continue;
                            }

                            while (client.TcpClient.Available > 0)
                            {
                                client.LastMessageTime = DateTime.Now;

                                client.Stream.Read(readBuffer, 0, readBuffer.Length);

                                readStream.Seek(0, SeekOrigin.Begin);
                                var header = reader.ReadHeader();

                                if (header.Type == MessageType.Ping)
                                {
                                    Debug.Log($"[Server] Ping received from client {client.Id}.");
                                    this.writeStream.Position = 0;
                                    this.writer.WriteHeader(new MessageHeader(0, MessageType.Pong));
                                    this.SendMessage(client, this.writeStream);
                                }
                                else if (header.Type == MessageType.Pong)
                                {
                                    Debug.Log($"[Server] Pong received from client {client.Id}.");
                                }

                                try
                                {
                                    this.MessageReceived?.Invoke(client.Id, header, reader);
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogError($"[Server] Exception thrown on message received callback.\n{exception}");
                                }
                            }

                            this.clients[clientIndex] = client;
                        }

                        Thread.Sleep(1);
                    }
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

        public struct Client
        {
            public byte Id;
            public TcpClient TcpClient;
            public NetworkStream Stream;
            public DateTime LastMessageTime;

            public Client(byte id, TcpClient newClient)
            {
                this.Id = id;
                this.TcpClient = newClient;
                this.Stream = newClient.GetStream();
                this.LastMessageTime = DateTime.Now;
            }

            public override string ToString()
            {
                return $"Id:{this.Id}";
            }
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}

