using Gameplay;

namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    using System.Linq;

    public class RecipeLine : MonoBehaviour
    {
        public Text RecipeName;

        public Button CreateFactoryButton;
        public Button DestroyFactoryButton;
        public Text FactoryCount;
        public Text FactoryProductivity;

        public Button CraftRecipeButton;
        public Text CraftRecipeCount;
        public Text CraftRecipeProgress;

        private RecipeDefinition definition;
        private Gameplay.Player player;

        public RecipeDefinition Definition
        {
            get
            {
                return this.definition;
            }

            set
            {
                this.definition = value;
                this.RecipeName.text = this.definition.name;
                this.GetComponentInChildren<TooltipInteractible>().Data = value;
            }
        }

        public void CraftRecipe()
        {
            this.player.CraftRecipe(this.Definition);
        }

        public void CreateFactory()
        {
            this.player.CreateFactory(this.Definition);
        }

        public void DestroyFactory()
        {
            this.player.DestroyFactory(this.Definition);
        }

        private void Awake()
        {
            Debug.Assert(this.RecipeName != null);
            Debug.Assert(this.CreateFactoryButton != null);
            Debug.Assert(this.DestroyFactoryButton != null);
            Debug.Assert(this.FactoryCount != null);
            Debug.Assert(this.FactoryProductivity != null);
            Debug.Assert(this.CraftRecipeButton != null);
            Debug.Assert(this.CraftRecipeCount != null);
            Debug.Assert(this.CraftRecipeProgress != null);
        }

        private void Start()
        {
            this.player = Gameplay.Game.Instance.Players[0];
        }

        private void Update()
        {
            this.CreateFactoryButton.interactable = player.CanCreateFactory(this.Definition);
            this.DestroyFactoryButton.interactable = player.CanDestroyFactory(this.Definition);
            this.CraftRecipeButton.interactable = player.CanCraftRecipe(this.Definition);

            var factory = this.player.Factories.Find(match => match.Definition == this.definition);
            if (factory != null)
            {
                this.FactoryCount.text = factory.Count.ToString();
                this.FactoryProductivity.text = ((float)factory.Productivity).ToString("P0");
            }
            else
            {
                this.FactoryCount.text = "0";
                this.FactoryProductivity.text = string.Empty;
            }

            var craftTaskIndex = this.player.ConstructionQueue.FindIndex(match => match.Definition == definition);
            if (craftTaskIndex >= 0)
            {
                bool isInProgress = craftTaskIndex == 0;
                var count = this.player.ConstructionQueue.Count(match => match.Definition == definition);

                if (isInProgress)
                {
                    this.CraftRecipeCount.text = count > 1 ? $"{count}x" : string.Empty;
                    this.CraftRecipeProgress.text = ((float)this.player.ConstructionQueue[craftTaskIndex].Progress).ToString("P0");
                }
                else
                {
                    this.CraftRecipeCount.text = count > 0 ? $"{count}x" : string.Empty;
                    this.CraftRecipeProgress.text = string.Empty;
                }
            }
            else
            {
                this.CraftRecipeCount.text = string.Empty;
                this.CraftRecipeProgress.text = string.Empty;
            }
        }
    }
}
