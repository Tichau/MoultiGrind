using System.Collections.Generic;
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

    public void StartGameServer()
    {
        this.server = new GameServer(System.Net.IPAddress.Parse("127.0.0.1"), this.networkPort, System.TimeSpan.FromSeconds(this.clientConnectionCheckTimeoutInSeconds), this.durationBetweenTwoTicks);
        this.server.Start();
    }

    public void ConnectToLocalServer()
    {
        this.client = new GameClient("localhost", this.networkPort);
        this.client.Start();
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
