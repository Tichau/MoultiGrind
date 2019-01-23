using Framework.Network;

namespace Network
{
    using UnityEngine;
    using UnityEngine.Networking;

    public class NetworkTest : MonoBehaviour
    {
        public bool isAtStartup = true;
        private Client client;
        private Server server;

        // Create a server and listen on a port
        public void SetupServer()
        {
            this.server = new Server();
            this.server.Start();
            isAtStartup = false;
        }

        // Create a client and connect to the server port
        public void SetupClient()
        {
            this.client = new Client();
            this.client.Start();
            //myClient.RegisterHandler(MsgType.Connect, OnConnected);
            //myClient.Connect("127.0.0.1", 4444);
            isAtStartup = false;
        }

        // client function
        public void OnConnected(NetworkMessage netMsg)
        {
            Debug.Log("Connected to server");
        }

        private void OnApplicationQuit()
        {
            this.client?.Stop();
            this.server?.Stop();
        }

        private void Update()
        {
            if (isAtStartup)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    SetupServer();
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    SetupClient();
                }
                if (Input.GetKeyDown(KeyCode.B))
                {
                    SetupServer();
                    SetupClient();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (Input.GetKeyDown(KeyCode.LeftControl))
                    {
                        this.server?.BroadcastAll(Message.Text("Plop"));
                    }
                    else
                    {
                        this.client?.SendMessage(Message.Text("Plip"));
                    }
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    this.client?.Stop();
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    this.server?.Stop();
                }
                if (Input.GetKeyDown(KeyCode.B))
                {
                    this.client?.Stop();
                    this.server?.Stop();
                }
            }
        }

        private void OnGUI()
        {
            if (isAtStartup)
            {
                GUI.Label(new Rect(2, 10, 150, 100), "Press S for server");
                GUI.Label(new Rect(2, 30, 150, 100), "Press B for both");
                GUI.Label(new Rect(2, 50, 150, 100), "Press C for client");
            }
        }
    }
}
