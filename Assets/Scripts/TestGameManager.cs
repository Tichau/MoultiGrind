using System.Collections.Generic;
using Simulation;
using Simulation.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestGameManager : MonoBehaviour
{
    private GameServer server;
    private GameClient client;

    [SerializeField]
    private uint durationBetweenTwoTicks = 1000;
    [SerializeField]
    private ulong timeElapsedPerTick = 1;

    private async void Start()
    {
        this.server = new GameServer(this.durationBetweenTwoTicks);
        this.server.Start();

        this.client = new GameClient();
        this.client.Start();

        var gameInstanceId = await this.client.PostCreateGameOrder(this.timeElapsedPerTick);
        await this.client.PostJoinGameOrder(gameInstanceId);

        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
    }

    private void Update()
    {
        this.client?.Update();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameClient.Instance.ActivePlayer.PostCreditResourcesOrder(ResourceType.AssemblingMachine1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            GameClient.Instance.ActivePlayer.PostCreditResourcesOrder(ResourceType.SciencePack1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            //this.TimeElapsedPerTick *= 2;
        }
        else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            //if (this.TimeElapsedPerTick > 1)
            //{
            //    this.TimeElapsedPerTick /= 2;
            //}
        }
#endif
    }

    private void OnApplicationQuit()
    {
        this.client?.Stop();
        this.server?.Stop();
    }
}
