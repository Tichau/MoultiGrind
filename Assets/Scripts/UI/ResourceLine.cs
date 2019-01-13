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
        private global::Player activePlayer;

        public ResourceType ResourceType
        {
            get
            {
                return this.resourceType;
            }

            set
            {
                this.resourceType = value;
                this.Name.text = this.resourceType.ToString();
            }
        }

        private void Awake()
        {
            Debug.Assert(this.Name != null);
            Debug.Assert(this.Amount != null);
            Debug.Assert(this.Net != null);

            this.activePlayer = Game.Instance.Players[0];
        }

        private void Update()
        {
            var amount = this.activePlayer.Resources[(int) this.resourceType].Amount;
            var net = this.activePlayer.Resources[(int)this.resourceType].Net;

            this.Amount.text = amount.ToString();

            if (net == Number.Zero)
            {
                this.Net.text = string.Empty;
            }
            else if (net > Number.Zero)
            {
                this.Net.color = Color.green;
                this.Net.text = net.ToString(true);
            }
            else
            {
                this.Net.color = Color.red;
                this.Net.text = net.ToString(true);
            }
        }
    }
}
