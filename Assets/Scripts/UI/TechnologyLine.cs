using Gameplay;

namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class TechnologyLine : MonoBehaviour
    {
        public Text Name;
        public Button ResearchButton;

        private TechnologyDefinition definition;

        public TechnologyDefinition Definition
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
            Gameplay.Game.Instance.Players[0].ResearchTechnology(this.Definition);
        }
        
        private void Awake()
        {
            Debug.Assert(this.Name != null);
            Debug.Assert(this.ResearchButton != null);
        }

        private void Update()
        {
            this.ResearchButton.interactable = Gameplay.Game.Instance.Players[0].CanResearchTechnology(this.Definition);
        }
    }
}
