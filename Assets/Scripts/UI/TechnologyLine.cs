using Simulation;
using Simulation.Network;

namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class TechnologyLine : MonoBehaviour
    {
        public Text Name;
        public Button ResearchButton;

        private Simulation.Data.TechnologyDefinition definition;

        public Simulation.Data.TechnologyDefinition Definition
        {
            get
            {
                return this.definition;
            }

            set
            {
                this.definition = value;
                this.GetComponentInChildren<TooltipInteractible>().Data = value;
                this.Name.text = this.definition.name;
            }
        }

        public void ResearchTechnology()
        {
            GameClient.Instance.ActivePlayer.PostResearchTechnologyOrder(this.Definition);
        }
        
        private void Awake()
        {
            Debug.Assert(this.Name != null);
            Debug.Assert(this.ResearchButton != null);
        }

        private void Update()
        {
            this.ResearchButton.interactable = GameClient.Instance.ActivePlayer.CanResearchTechnology(this.Definition);
        }
    }
}
