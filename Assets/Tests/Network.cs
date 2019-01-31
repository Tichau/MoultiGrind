using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Framework;
using Framework.Network;
using NUnit.Framework;
using Simulation;
using Simulation.Network;
using UnityEngine;
using UnityEngine.TestTools;

public class NetworkTest
{
    [Test]
    public void ServerAndClientCanConnectToEachOther()
    {
        using (Server server = new Server(IPAddress.Parse("127.0.0.1"), 8052, TimeSpan.FromMilliseconds(100)))
        {
            Assert.AreEqual(InterfaceState.None, server.State);
            server.Start();
            Assert.AreEqual(InterfaceState.Started, server.State);

            Assert.AreEqual(0, server.ClientCount);

            using (Client client1 = new Client("127.0.0.1", 8052))
            {
                Assert.AreEqual(InterfaceState.None, client1.State);
                client1.Start();
                Assert.AreEqual(InterfaceState.Started, client1.State);
                Thread.Sleep(50);
                Assert.AreEqual(1, server.ClientCount);
                Assert.AreEqual(0, client1.Id);

                using (Client client2 = new Client("127.0.0.1", 8052))
                {
                    client2.Start();
                    Thread.Sleep(50);
                    Assert.AreEqual(2, server.ClientCount);
                    Assert.AreEqual(1, client2.Id);
                }

                Thread.Sleep(100);
                Assert.AreEqual(1, server.ClientCount);

                using (Client client2 = new Client("127.0.0.1", 8052))
                {
                    client2.Start();
                    Thread.Sleep(50);
                    Assert.AreEqual(2, server.ClientCount);
                    Assert.AreEqual(2, client2.Id);

                }

                Thread.Sleep(100);
                Assert.AreEqual(1, server.ClientCount);

                Thread.Sleep(100);

                client1.Stop();
                Assert.AreEqual(InterfaceState.Stopped, client1.State);
            }

            // Wait for timeout after disconnection.
            Thread.Sleep(200);
            Assert.AreEqual(0, server.ClientCount);

            server.Stop();
            Assert.AreEqual(InterfaceState.Stopped, server.State);
        }
    }

    [Test]
    public void ServerAndClientCanCommunicate()
    {
        using (Server server = new Server(IPAddress.Parse("127.0.0.1"), 8052, TimeSpan.FromSeconds(1)))
        using (Client client = new Client("127.0.0.1", 8052))
        {
            string messageReceivedByServer = null;
            server.Start();
            server.MessageReceived += (id, header, buffer) =>
            {
                buffer.ReadTextMessage(header, out messageReceivedByServer);
            };

            string messageReceivedByClient = null;
            client.Start();
            client.MessageReceived += (header, buffer) =>
            {
                buffer.ReadTextMessage(header, out messageReceivedByClient);
            };

            Thread.Sleep(50);

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.WriteTextMessage("Client to server message.");

                client.SendMessage(stream);
                Thread.Sleep(100);
                Assert.AreEqual("Client to server message.", messageReceivedByServer.ToString());
            }

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.WriteTextMessage("Server to client message.");

                server.SendMessage(0, stream);
                Thread.Sleep(100);
                Assert.AreEqual("Server to client message.", messageReceivedByClient.ToString());
            }
        }
    }
}
