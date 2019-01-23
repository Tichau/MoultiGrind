using Simulation;

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
        }

        private void Update()
        {
            var amount = GameClient.Instance.ActivePlayer.Resources[(int) this.resourceType].Amount;
            var net = GameClient.Instance.ActivePlayer.Resources[(int)this.resourceType].Net;

            this.Amount.text = amount.ToString();
            this.Net.SetTextToSignedNumber(net);
        }
    }
}
