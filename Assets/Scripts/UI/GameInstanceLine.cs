namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    using Simulation.Network;

    public class GameInstanceLine : UIList<PlayerSlotLine>
    {
        public Text InstanceName;

        private GameInstanceSummary instance;
        private RectTransform rectTransform;

        public GameInstanceSummary Instance
        {
            get
            {
                return this.instance;
            }

            set
            {
                this.instance = value;
                this.InstanceName.text = $"Game {this.instance.Id}";

                this.DisplayList(this.instance.PlayerSlots, null, (summary, ui) =>
                {
                    ui.GameInstanceId = this.instance.Id;
                    ui.Slot = summary;
                });

                this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, 30 + this.Instance.PlayerSlots.Length * 40);
            }
        }

        public async void JoinAsNewPlayer()
        {
            Debug.Assert(GameClient.Instance != null);

            await GameClient.Instance.PostJoinGameOrder(this.Instance.Id);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void Awake()
        {
            Debug.Assert(this.InstanceName != null);
            rectTransform = this.GetComponent<RectTransform>();
        }
    }
}
