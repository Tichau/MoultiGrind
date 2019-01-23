using Framework;

namespace Simulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor;

    public class Player
    {
        public Resource[] Resources;
        public List<Factory> Factories = new List<Factory>();
        public List<CraftTask> ConstructionQueue = new List<CraftTask>();

        public Dictionary<TechnologyDefinition, ResearchStatus> TechnologyStatesByDefinition =
            new Dictionary<TechnologyDefinition, ResearchStatus>();

        public Player()
        {
            Array enumValues = typeof(ResourceType).GetEnumValues();
            this.Resources = new Resource[enumValues.Length];
            foreach (var enumValue in enumValues)
            {
                this.Resources[(int) enumValue] = new Resource((ResourceType) enumValue, Number.Zero);
            }

            foreach (var technology in Databases.Instance.TechnologyDefinitions)
            {
                this.TechnologyStatesByDefinition.Add(technology, ResearchStatus.Available);
            }
        }

        public void Tick(Number timeElapsed)
        {
            // Work on craft tasks
            Number workTime = timeElapsed;
            for (var index = 0; index < this.ConstructionQueue.Count && workTime > Number.Zero; index++)
            {
                var craftTask = this.ConstructionQueue[index];
                Number timeToSpend = Number.Min(workTime, craftTask.Definition.Duration - craftTask.TimeSpent);
                craftTask.TimeSpent += timeToSpend;
                workTime -= timeToSpend;

                if (craftTask.Progress >= new Number(1))
                {
                    // Craft task terminated.
                    foreach (var resource in craftTask.Definition.Outputs)
                    {
                        this.Resources[(int) resource.Name].Amount += resource.Amount;
                    }

                    this.ConstructionQueue.RemoveAt(index);
                    index--;
                }
            }

            // Compute needed amount per resource.
            for (int index = 0; index < this.Resources.Length; index++)
            {
                this.Resources[index].AmountNeeded = new Number(0);
                this.Resources[index].AmountToDebit = new Number(0);
                this.Resources[index].NetOperations.Clear();
            }

            // Compute total needed amount of resources.
            foreach (var factory in this.Factories)
            {
                foreach (var resource in factory.Definition.Inputs)
                {
                    Number amountPerFactory = resource.Amount * timeElapsed / factory.Definition.Duration;
                    this.Resources[(int) resource.Name].AmountNeeded += amountPerFactory * factory.Count;
                }
            }

            // Compute factories productivity.
            foreach (var factory in this.Factories)
            {
                factory.Productivity = new Number(1);
                foreach (var resource in factory.Definition.Inputs)
                {
                    factory.Productivity = Number.Min(factory.Productivity,
                        this.Resources[(int) resource.Name].SpendableNeededAmountPercent);
                }

                foreach (var resource in factory.Definition.Inputs)
                {
                    Number amountPerFactory = resource.Amount * timeElapsed / factory.Definition.Duration;
                    this.Resources[(int) resource.Name].AmountToDebit +=
                        amountPerFactory * factory.Productivity * factory.Count;
                }
            }

            // Cut off needs and gather remaining resources.
            for (int index = 0; index < this.Resources.Length; index++)
            {
                this.Resources[index].Annotation("Production", this.Resources[index].NetFromPreviousTick);
                this.Resources[index].Annotation("Consumption", new Number(-1) * this.Resources[index].AmountToDebit);

                // Cut off needs (inputs) from what we produce (raw output from previous tick) and from amount.
                var upkeep = Number.Min(this.Resources[index].NetFromPreviousTick, this.Resources[index].AmountToDebit);
                var stockDebit =
                    Number.Max(this.Resources[index].AmountToDebit - this.Resources[index].NetFromPreviousTick,
                        new Number(0));
                this.Resources[index].Amount -= stockDebit;
                this.Resources[index].NetFromPreviousTick -= upkeep;

                // Compute the net (what is credited on the stock each turn).
                this.Resources[index].Net = this.Resources[index].NetFromPreviousTick - stockDebit;

                // Gather remaining resources from previous tick in stock.
                this.Resources[index].Amount += this.Resources[index].NetFromPreviousTick;
                this.Resources[index].NetFromPreviousTick = new Number(0);
            }

            // Compute raw output (that will be used for next tick).
            foreach (var factory in this.Factories)
            {
                foreach (var resource in factory.Definition.Outputs)
                {
                    Number amountPerFactory = resource.Amount * timeElapsed / factory.Definition.Duration;
                    this.Resources[(int) resource.Name].NetFromPreviousTick +=
                        amountPerFactory * factory.Productivity * factory.Count;
                }
            }
        }

        public void UnTick(Number timeElapsedPerTick)
        {
            throw new NotImplementedException();
        }

        public bool IsRecipeAvailable(RecipeDefinition definition)
        {
            foreach (var technology in TechnologyStatesByDefinition)
            {
                foreach (var unlock in technology.Key.Unlocks)
                {
                    if (unlock == definition)
                    {
                        return technology.Value == ResearchStatus.Done;
                    }
                }
            }

            return true;
        }

        public bool CanCreateFactory(RecipeDefinition definition)
        {
            if (this.Resources[(int) ResourceType.AssemblingMachine1].Amount < new Number(1))
            {
                return false;
            }

            return true;
        }

        public void CreateFactory(RecipeDefinition definition)
        {
            if (!this.CanCreateFactory(definition))
            {
                return;
            }

            this.Resources[(int) ResourceType.AssemblingMachine1].Amount -= new Number(1);

            var factory = this.Factories.Find(match => match.Definition == definition);
            if (factory == null)
            {
                factory = new Factory(definition);
                this.Factories.Add(factory);
            }

            factory.Count++;
        }

        public bool CanDestroyFactory(RecipeDefinition definition)
        {
            return this.Factories.Any(factory => factory.Definition == definition && factory.Count > 0);
        }

        public void DestroyFactory(RecipeDefinition definition)
        {
            if (!this.CanDestroyFactory(definition))
            {
                return;
            }

            this.Resources[(int) ResourceType.AssemblingMachine1].Amount += new Number(1);

            var factory = this.Factories.Find(match => match.Definition == definition);
            factory.Count--;
        }

        public bool CanCraftRecipe(RecipeDefinition definition)
        {
            bool resourcePrerequisites = true;
            foreach (var resource in definition.Inputs)
            {
                resourcePrerequisites &= resource.Amount <= this.Resources[(int) resource.Name].Amount;
            }

            return resourcePrerequisites;
        }

        public void CraftRecipe(RecipeDefinition definition)
        {
            if (!this.CanCraftRecipe(definition))
            {
                return;
            }

            foreach (var resource in definition.Inputs)
            {
                this.Resources[(int) resource.Name].Amount -= resource.Amount;
            }

            this.ConstructionQueue.Add(new CraftTask(definition));
        }

        public bool CanResearchTechnology(TechnologyDefinition definition)
        {
            if (this.TechnologyStatesByDefinition[definition] != ResearchStatus.Available)
            {
                return false;
            }

            bool resourcePrerequisites = true;
            foreach (var resource in definition.Costs)
            {
                resourcePrerequisites &= resource.Amount <= this.Resources[(int) resource.Name].Amount;
            }

            return resourcePrerequisites;
        }

        public void ResearchTechnology(TechnologyDefinition definition)
        {
            if (!this.CanResearchTechnology(definition))
            {
                return;
            }

            foreach (var resource in definition.Costs)
            {
                this.Resources[(int) resource.Name].Amount -= resource.Amount;
            }

            this.TechnologyStatesByDefinition[definition] = ResearchStatus.Done;
        }
    }
}
