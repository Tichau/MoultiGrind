using Gameplay;

namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ResourceLine : MonoBehaviour
    {
        public Text Name;
        public Text Amount;
        public Text Net;

        private ResourceType resourceType;
        private Gameplay.Player activePlayer;

        public ResourceType ResourceType
        {
            get
            {
                return this.resourceType;
            }

            set
            {
                this.resourceType = value;
                this.GetComponent<TooltipInteractible>().Data = this.resourceType;
                this.Name.text = this.resourceType.ToString();
            }
        }

        private void Awake()
        {
            Debug.Assert(this.Name != null);
            Debug.Assert(this.Amount != null);
            Debug.Assert(this.Net != null);

            this.activePlayer = Gameplay.Game.Instance.Players[0];
        }

        private void Update()
        {
            var amount = this.activePlayer.Resources[(int) this.resourceType].Amount;
            var net = this.activePlayer.Resources[(int)this.resourceType].Net;

            this.Amount.text = amount.ToString();
            this.Net.SetTextToSignedNumber(net);
        }
    }
}
