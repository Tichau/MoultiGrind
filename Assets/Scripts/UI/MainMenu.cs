using System.Net;

namespace UI
{
    using UnityEngine;

    public class MainMenu : MonoBehaviour
    {
        public async void SinglePlayer()
        {
            GameManager.Instance.StartGameServer(IPAddress.Parse("127.0.0.1"));
            GameManager.Instance.ConnectToServer("localhost");

            Debug.Assert(Simulation.Network.GameClient.Instance != null);

            var gameInstanceId = await Simulation.Network.GameClient.Instance.PostCreateGameOrder(1);
            await Simulation.Network.GameClient.Instance.PostJoinGameOrder(gameInstanceId);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            GameManager.Instance.Quit();
        }
    }
}
