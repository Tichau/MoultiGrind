using System.Collections;
using System.Collections.Generic;
using Simulation;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class TooltipResourceLine : MonoBehaviour
{
    public Text Name;
    public Text Amount;

    public object Definition
    {
        set
        {
            if (value is Simulation.Data.ResourceDefinition resourceDefinition)
            {
                this.Name.text = resourceDefinition.Name.ToString();
                this.Amount.text = resourceDefinition.Amount.ToString();
            }
            else if (value is Simulation.Data.RecipeDefinition recipeDefinition)
            {
                this.Name.text = recipeDefinition.name;
                this.Amount.text = string.Empty;
            }
            else if (value is Operation operation)
            {
                this.Name.text = operation.Name;
                this.Amount.SetTextToSignedNumber(operation.Amount);
            }
        }
    }

    private void Awake()
    {
        Debug.Assert(this.Name != null);
        Debug.Assert(this.Amount != null);
    }
}
