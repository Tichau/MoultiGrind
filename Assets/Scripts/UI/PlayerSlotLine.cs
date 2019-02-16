namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    using Simulation.Network;

    public class PlayerSlotLine : MonoBehaviour
    {
        public Text SlotName;
        public Button JoinButton;

        private PlayerSlotSummary slot;

        public byte GameInstanceId
        {
            get;
            set;
        }

        public PlayerSlotSummary Slot
        {
            get
            {
                return this.slot;
            }

            set
            {
                this.slot = value;
                this.SlotName.text = $"Player {this.slot.Id}";
            }
        }

        public async void Join()
        {
            Debug.Assert(GameClient.Instance != null);

            await GameClient.Instance.PostJoinGameOrder(this.GameInstanceId, this.Slot.Id);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Game", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void Awake()
        {
            Debug.Assert(this.SlotName != null);
            Debug.Assert(this.JoinButton != null);
        }
    }
}
