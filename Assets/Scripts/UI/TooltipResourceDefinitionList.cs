namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TooltipResourceDefinitionList : UIList<TooltipResourceLine>
    {
        public IEnumerable<object> Definitions { get; set; }

        private void Update()
        {
            this.DisplayList(this.Definitions, null, (def, ui) => ui.Definition = def);
        }
    }
}
