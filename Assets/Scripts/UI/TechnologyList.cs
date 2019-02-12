using Simulation;
using Simulation.Network;

namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TechnologyList : UIList<TechnologyLine>
    {
        private Predicate<TechnologyStatus> displayPredicate = status => status.Status == ResearchStatus.Available || status.Status == ResearchStatus.InProgress;

        private void Update()
        {
            // Display factories
            this.DisplayList(GameClient.Instance.ActivePlayer.TechnologyStatusById, this.displayPredicate, (def, ui) => ui.Definition = def.Definition);
        }
    }
}
