﻿namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class TechnologyButton : MonoBehaviour
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
                this.Name.text = this.definition.name;
            }
        }

        public void ResearchTechnology()
        {
            Game.Instance.Players[0].ResearchTechnology(this.Definition);
        }
        
        private void Awake()
        {
            Debug.Assert(this.Name != null);
            Debug.Assert(this.ResearchButton != null);
        }

        private void Update()
        {
            this.ResearchButton.interactable = Game.Instance.Players[0].CanResearchTechnology(this.Definition);
        }
    }
}
