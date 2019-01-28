using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Framework;
using Framework.Network;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NetworkTest
{
    [Test]
    public void ServerAndClientCanConnectToEachOther()
    {
        using (Server server = new Server(IPAddress.Parse("127.0.0.1"), 8052, TimeSpan.FromMilliseconds(100)))
        {
            server.Start();

            Assert.AreEqual(0, server.ClientCount);

            using (Client client = new Client("127.0.0.1", 8052))
            {
                client.Start();
                Thread.Sleep(50);
                Assert.AreEqual(1, server.ClientCount);

                Thread.Sleep(100);
            }

            // Wait for timeout after disconnection.
            Thread.Sleep(200);
            Assert.AreEqual(0, server.ClientCount);
        }
    }

    [Test]
    public void ServerAndClientCanCommunicate()
    {
        using (Server server = new Server(IPAddress.Parse("127.0.0.1"), 8052, TimeSpan.FromSeconds(1)))
        using (Client client = new Client("127.0.0.1", 8052))
        {
            Message messageReceivedByServer = Message.Invalid;
            server.Start();
            server.MessageReceived += (id, header, buffer) =>
            {
                messageReceivedByServer = buffer.ReadMessage(header);
            };

            Message messageReceivedByClient = Message.Invalid;
            client.Start();
            client.MessageReceived += (header, buffer) =>
            {
                messageReceivedByClient = buffer.ReadMessage(header);
            };

            Thread.Sleep(50);

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Message message = Message.Text("Client to server message.");
                writer.WriteMessage(message);

                client.SendMessage(stream);
                Thread.Sleep(100);
                Assert.AreEqual("[25B] Text: Client to server message.", messageReceivedByServer.ToString());
            }

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Message message = Message.Text("Server to client message.");
                writer.WriteMessage(message);

                server.SendMessage(0, stream);
                Thread.Sleep(100);
                Assert.AreEqual("[25B] Text: Server to client message.", messageReceivedByClient.ToString());
            }
        }
    }
}
