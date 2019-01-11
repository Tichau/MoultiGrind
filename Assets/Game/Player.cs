using System;
using System.Collections.Generic;
using UnityEditor;

public class Player
{
    public Resource[] Resources;
    public Dictionary<RecipeDefinition, Factory> Factories = new Dictionary<RecipeDefinition, Factory>();
    
    public Player()
    {
        Array enumValues = typeof(ResourceType).GetEnumValues();
        this.Resources = new Resource[enumValues.Length];
        foreach (var enumValue in enumValues)
        {
            this.Resources[(int)enumValue] = new Resource((ResourceType)enumValue, Number.Zero);
        }
    }

    public void Tick(Number timeElapsed)
    {
        // Compute needed amount per resource.
        for (int index = 0; index < this.Resources.Length; index++)
        {
            this.Resources[index].AmountNeeded = new Number(0);
        }
        
        // Compute total needed amount of resources.
        foreach (var factory in this.Factories.Values)
        {
            foreach (var resource in factory.Definition.Inputs)
            {
                Number amountPerFactory = resource.Amount * timeElapsed / factory.Definition.Duration;
                this.Resources[(int)resource.Name].AmountNeeded += amountPerFactory * factory.Count;
            }
        }

        // Compute factories productivity.
        foreach (var factory in this.Factories.Values)
        {
            factory.Productivity = new Number(1);
            foreach (var resource in factory.Definition.Inputs)
            {
                factory.Productivity = Number.Min(factory.Productivity, this.Resources[(int)resource.Name].SpendableNeededAmountPercent);
            }
        }

        // Cut off needs and gather remaining resources.
        for (int index = 0; index < this.Resources.Length; index++)
        {
            // Cut off needs (inputs) from what we produce (raw output from previous tick) and from amount.
            var upkeep = Number.Min(this.Resources[index].NetFromPreviousTick, this.Resources[index].AmountToSpend);
            var stockDebit = Number.Max(this.Resources[index].AmountToSpend - this.Resources[index].NetFromPreviousTick, new Number(0));
            this.Resources[index].Amount -= stockDebit;
            this.Resources[index].NetFromPreviousTick -= upkeep;

            // Compute the net (what is credited on the stock each turn).
            this.Resources[index].Net = this.Resources[index].NetFromPreviousTick - stockDebit;

            // Gather remaining resources from previous tick in stock.
            this.Resources[index].Amount += this.Resources[index].NetFromPreviousTick;
            this.Resources[index].NetFromPreviousTick = new Number(0);
        }

        // Compute raw output (that will be used for next tick).
        foreach (var factory in this.Factories.Values)
        {
            foreach (var resource in factory.Definition.Outputs)
            {
                Number amountPerFactory = resource.Amount * timeElapsed / factory.Definition.Duration;
                this.Resources[(int)resource.Name].NetFromPreviousTick += amountPerFactory * factory.Productivity * factory.Count;
            }
        }
    }

    public void CreateFactory(RecipeDefinition definition)
    {
        if (!this.Factories.ContainsKey(definition))
        {
            this.Factories.Add(definition, new Factory(definition));
        }

        this.Factories[definition].Count++;
    }
}