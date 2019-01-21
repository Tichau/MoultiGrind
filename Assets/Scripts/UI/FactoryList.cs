namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Gameplay;

    public class FactoryList : UIList<RecipeLine>
    {
        private Predicate<RecipeDefinition> displayPredicate = def => Gameplay.Game.Instance.Players[0].IsRecipeAvailable(def);
        
        private void Update()
        {
            // Buildable factories
            this.DisplayList(Databases.Instance.RecipeDefinitions, this.displayPredicate, (def, ui) => ui.Definition = def);
        }
    }
}
