namespace UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class RecipeButtons : MonoBehaviour
    {
        public Text RecipeName;
        public Button CraftButton;
        public Button FactoryButton;

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
            }
        }

        public void CraftRecipe()
        {
            Game.Instance.Players[0].CraftRecipe(this.Definition);
        }

        public void CreateFactory()
        {
            Game.Instance.Players[0].CreateFactory(this.Definition);
        }

        private void Awake()
        {
            Debug.Assert(this.RecipeName != null);
            Debug.Assert(this.CraftButton != null);
        }

        private void Update()
        {
            this.CraftButton.interactable = Game.Instance.Players[0].CanCraftRecipe(this.Definition);
        }
    }
}
