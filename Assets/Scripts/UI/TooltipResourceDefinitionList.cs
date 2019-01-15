namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TooltipResourceDefinitionList : UIList<TooltipResourceLine>
    {
        public ResourceDefinition[] ResourceDefinitions { get; set; }

        private void Update()
        {
            this.DisplayList(this.ResourceDefinitions, null, (def, ui) => ui.ResourceDefinition = def);
        }
    }
}
