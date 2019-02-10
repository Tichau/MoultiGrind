using Simulation.Network;

namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Simulation;

    public class FactoryList : UIList<RecipeLine>
    {
        private Predicate<Simulation.Data.RecipeDefinition> displayPredicate = def => GameClient.Instance.ActivePlayer.IsRecipeAvailable(def);
        
        private void Update()
        {
            // Buildable factories
            this.DisplayList(Databases.Instance.RecipeDefinitions, this.displayPredicate, (def, ui) => ui.Definition = def);
        }
    }
}
