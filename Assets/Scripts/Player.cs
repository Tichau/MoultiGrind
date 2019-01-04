using System;
using System.Collections.Generic;

using DefaultNamespace;

public class Player
{
    public Dictionary<ResourceType, float> Resources = new Dictionary<ResourceType, float>();
    public Dictionary<FactoryDefinition, Factory> Factories = new Dictionary<FactoryDefinition, Factory>();
    
    public Player()
    {
        Array enumValues = typeof(ResourceType).GetEnumValues();
        foreach (var enumValue in enumValues)
        {
            this.Resources.Add((ResourceType)enumValue, 0);
        }
    }

    public void Tick()
    {
        foreach (var factory in this.Factories.Values)
        {
            factory.ComputeOutput(this);
        }
    }

    public void CreateFactory(FactoryDefinition definition)
    {
        if (!this.Factories.ContainsKey(definition))
        {
            this.Factories.Add(definition, new Factory(definition));
        }

        this.Factories[definition].Count++;
    }
}