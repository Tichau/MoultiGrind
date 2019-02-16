using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Simulation;
using Simulation.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private uint durationBetweenTwoTicks = 1000;

    [SerializeField]
    private int networkPort = 7744;

    [SerializeField]
    private int clientConnectionCheckTimeoutInSeconds = 30;

    private GameServer server;
    private GameClient client;

    private void Awake()
    {
        Debug.Assert(GameManager.Instance == null, "Instance should be null before assignation.");
        GameManager.Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void StartGameServer(IPAddress address)
    {
        this.StartGameServer(address, this.networkPort);
    }

    public void StartGameServer(IPAddress address, int port)
    {
        Debug.Assert(this.server == null);
        this.server = new GameServer(address, port, System.TimeSpan.FromSeconds(this.clientConnectionCheckTimeoutInSeconds), this.durationBetweenTwoTicks);
        this.server.Start();
    }

    public void ConnectToServer(string address)
    {
        this.ConnectToServer(address, this.networkPort);
    }

    public void ConnectToServer(string address, int port)
    {
        this.client = new GameClient(address, port);
        this.client.Start();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
            Application.OpenURL(webplayerQuitURL);
#else
            Application.Quit();
#endif
    }

    private void Update()
    {
        this.client?.Update();
    }

    private void OnApplicationQuit()
    {
        this.client?.Stop();
        this.server?.Stop();
    }
}
