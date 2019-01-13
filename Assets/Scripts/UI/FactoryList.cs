namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FactoryList : UIList<RecipeLine>
    {
        private Predicate<RecipeDefinition> displayPredicate = def => Game.Instance.Players[0].IsRecipeAvailable(def);
        
        private void Update()
        {
            // Buildable factories
            this.DisplayList(Game.Instance.RecipeDefinitions, this.displayPredicate, (def, ui) => ui.Definition = def);
        }
    }
}
