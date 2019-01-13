namespace UI
{
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    public class ResourceList : UIList<ResourceLine>
    {
        private void Update()
        {
            // Only display player 0 for now.
            var player = Game.Instance.Players[0];
            this.DisplayList(player.Resources, null, (def, ui) => ui.ResourceType = def.Name);
        }
    }
}
