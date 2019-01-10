using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Factory", menuName = "Factory Definition", order = 1)]
public class FactoryDefinition : ScriptableObject
{
    public string Name = "New Factory";

    public ResourceDefinition[] Inputs;
    public ResourceDefinition[] Outputs;
}

[Serializable]
public struct ResourceDefinition
{
    public ResourceType Name;

    [SerializeField]
    private long fixedPointAmount;

    public Number Amount => Number.FromFixedPoint(this.fixedPointAmount);
}
