using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Recipe Definition", order = 1)]
public class RecipeDefinition : ScriptableObject
{
    public string Name = "New Recipe";

    public ResourceDefinition[] Inputs;
    public ResourceDefinition[] Outputs;

    [SerializeField]
    private long fixedPointDuration;

    public Number Duration => Number.FromFixedPoint(this.fixedPointDuration);
}

[Serializable]
public struct ResourceDefinition
{
    public ResourceType Name;

    [SerializeField]
    private long fixedPointAmount;

    public Number Amount => Number.FromFixedPoint(this.fixedPointAmount);
}
