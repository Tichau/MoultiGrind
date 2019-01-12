namespace UI
{
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;

    public class RecipeButtons : MonoBehaviour
    {
        public Text RecipeName;

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
                this.RecipeName.text = this.definition.Name;
            }
        }

        private void Awake()
        {
            Debug.Assert(this.RecipeName != null);
        }

        public void CraftRecipe()
        {
            Game.Instance.Players[0].CraftRecipe(this.Definition);
        }

        public void CreateFactory()
        {
            Game.Instance.Players[0].CreateFactory(this.Definition);
        }
    }
}
