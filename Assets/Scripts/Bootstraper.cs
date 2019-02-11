using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstraper : MonoBehaviour
{
    public string GameScene;

    private bool headlessMode = false;

    private void Start()
    {
        var args = System.Environment.GetCommandLineArgs();
        string hostName = null;
        int serverPort = -1;
        for (int index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (!argument.StartsWith("--"))
            {
                continue;
            }

            var argName = argument.Substring(2);
            switch (argName)
            {
                case "headless-server":
                    this.headlessMode = true;
                    break;

                case "host-name":
                    Debug.Assert(index + 1 < args.Length);
                    hostName = args[++index];
                    break;

                case "server-port":
                    Debug.Assert(index + 1 < args.Length);
                    if (!int.TryParse(args[++index], out serverPort))
                    {
                        Debug.LogError("Invalid server port format.");
                    }

                    break;
            }
        }
        
        if (this.headlessMode)
        {
            this.StartHeadlessServer(hostName, serverPort);
        }
        else
        {
            this.StartGame();
        }
    }
    
    private void StartHeadlessServer(string hostName, int serverPort)
    {
        if (string.IsNullOrEmpty(hostName))
        {
            Debug.Log("No host name specified, set up server on 'localhost'.");
            hostName = "localhost";
        }

        var hostAddresses = Dns.GetHostAddresses(hostName);
        if (hostAddresses.Length == 0)
        {
            Debug.LogError($"No address found for host name {hostName}");
            GameManager.Instance.Quit();
            return;
        }

        IPAddress ipAddress = hostAddresses[0];

        if (serverPort < 0)
        {
            GameManager.Instance.StartGameServer(ipAddress);
        }
        else
        {
            GameManager.Instance.StartGameServer(ipAddress, serverPort);
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene(this.GameScene, LoadSceneMode.Single);
    }
}
