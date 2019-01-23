using Simulation;

namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TechnologyList : UIList<TechnologyLine>
    {
        private Predicate<KeyValuePair<TechnologyDefinition, ResearchStatus>> displayPredicate = def => def.Value == ResearchStatus.Available || def.Value == ResearchStatus.InProgress;

        private void Update()
        {
            // Display factories
            this.DisplayList(GameClient.Instance.ActivePlayer.TechnologyStatesByDefinition, this.displayPredicate, (def, ui) => ui.Definition = def.Key);
        }
    }
}
