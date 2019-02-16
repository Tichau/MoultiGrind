
using Simulation;
using Simulation.Network;
using UnityEngine.UI;

namespace UI
{
    using UnityEngine;

    public class MultiplayerMenu : UIList<GameInstanceLine>
    {
        public InputField ServerAddress;
        public Button ConnectButton;
        public RectTransform GameInstanceListContent;

        private GameInstanceSummary[] gameInstanceSummaries;

        public async void ConnectToServer()
        {
            var serverAddress = this.ServerAddress.text;
            GameManager.Instance.ConnectToServer(serverAddress);

            this.gameInstanceSummaries = await GameClient.Instance.PostListGamesOrder();

            this.UpdateGameSummaries();
        }

        public async void CreateAndJoinGame()
        {
            Debug.Assert(Simulation.Network.GameClient.Instance != null);

            var gameInstanceId = await GameClient.Instance.PostCreateGameOrder(1);
            await GameClient.Instance.PostJoinGameOrder(gameInstanceId);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void Update()
        {
        }

        private void UpdateGameSummaries()
        {
            this.DisplayList(this.gameInstanceSummaries, null, (def, ui) => ui.Instance = def);
        }
    }
}
