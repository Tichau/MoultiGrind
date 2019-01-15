using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipResourceLine : MonoBehaviour
{
    public Text Name;
    public Text Amount;

    private ResourceDefinition resourceDefinition;

    public ResourceDefinition ResourceDefinition
    {
        get
        {
            return this.resourceDefinition;
        }

        set
        {
            this.resourceDefinition = value;
            this.Name.text = this.resourceDefinition.Name.ToString();
            this.Amount.text = this.resourceDefinition.Amount.ToString();
        }
    }

    private void Awake()
    {
        Debug.Assert(this.Name != null);
        Debug.Assert(this.Amount != null);
    }
}
