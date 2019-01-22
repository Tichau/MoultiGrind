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
            GameClient.Instance.ActivePlayer.CraftRecipe(this.Definition);
        }

        public void CreateFactory()
        {
            GameClient.Instance.ActivePlayer.CreateFactory(this.Definition);
        }

        public void DestroyFactory()
        {
            GameClient.Instance.ActivePlayer.DestroyFactory(this.Definition);
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
        }

        private void Update()
        {
            this.CreateFactoryButton.interactable = GameClient.Instance.ActivePlayer.CanCreateFactory(this.Definition);
            this.DestroyFactoryButton.interactable = GameClient.Instance.ActivePlayer.CanDestroyFactory(this.Definition);
            this.CraftRecipeButton.interactable = GameClient.Instance.ActivePlayer.CanCraftRecipe(this.Definition);

            var factory = GameClient.Instance.ActivePlayer.Factories.Find(match => match.Definition == this.definition);
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

            var craftTaskIndex = GameClient.Instance.ActivePlayer.ConstructionQueue.FindIndex(match => match.Definition == definition);
            if (craftTaskIndex >= 0)
            {
                bool isInProgress = craftTaskIndex == 0;
                var count = GameClient.Instance.ActivePlayer.ConstructionQueue.Count(match => match.Definition == definition);

                if (isInProgress)
                {
                    this.CraftRecipeCount.text = count > 1 ? $"{count}x" : string.Empty;
                    this.CraftRecipeProgress.text = ((float)GameClient.Instance.ActivePlayer.ConstructionQueue[craftTaskIndex].Progress).ToString("P0");
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
