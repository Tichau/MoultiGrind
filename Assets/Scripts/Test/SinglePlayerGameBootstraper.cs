using System.Collections.Generic;
using Simulation;
using Simulation.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SinglePlayerGameBootstraper : MonoBehaviour
{
    [SerializeField]
    private ulong timeElapsedPerTick = 1;

    private async void Start()
    {
        GameManager.Instance.StartGameServer();
        GameManager.Instance.ConnectToLocalServer();

        Debug.Assert(Simulation.Network.GameClient.Instance != null);

        var gameInstanceId = await GameClient.Instance.PostCreateGameOrder(this.timeElapsedPerTick);
        await GameClient.Instance.PostJoinGameOrder(gameInstanceId);

        SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (GameClient.Instance.Game != null)
        {
            var gameTimeElapsedPerTick = (ulong)GameClient.Instance.Game.TimeElapsedPerTick;
            if (Input.GetKeyDown(KeyCode.A))
            {
                GameClient.Instance.ActivePlayer.PostCreditResourcesOrder(ResourceType.AssemblingMachine1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                GameClient.Instance.ActivePlayer.PostCreditResourcesOrder(ResourceType.SciencePack1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                GameClient.Instance.Game.PostChangeGameSpeedOrder(gameTimeElapsedPerTick * 2);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (gameTimeElapsedPerTick > 1)
                {
                    GameClient.Instance.Game.PostChangeGameSpeedOrder(gameTimeElapsedPerTick / 2);
                }
            }
        }
#endif
    }
}
