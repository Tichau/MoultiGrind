namespace UI
{
    using UnityEngine;

    public class MainMenu : MonoBehaviour
    {
        public async void NewGame()
        {
            GameManager.Instance.StartGameServer();
            GameManager.Instance.ConnectToLocalServer();

            Debug.Assert(Simulation.Network.GameClient.Instance != null);

            var gameInstanceId = await Simulation.Network.GameClient.Instance.PostCreateGameOrder(1);
            await Simulation.Network.GameClient.Instance.PostJoinGameOrder(gameInstanceId);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
            Application.OpenURL(webplayerQuitURL);
#else
            Application.Quit();
#endif
        }
    }
}
