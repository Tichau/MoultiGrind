using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Framework;
using Simulation.Network;
using UnityEngine;

namespace Simulation.Player
{
    public partial class Player : ISerializable
    {
        public Resource[] Resources;
        public List<Factory> Factories = new List<Factory>();
        public List<CraftTask> ConstructionQueue = new List<CraftTask>();
        public TechnologyStatus[] TechnologyStatusById;

        internal byte ClientId;
        internal OrderData[] OrderById;

        internal Player(byte clientId)
        {
            this.GenerateOrderData();

            this.ClientId = clientId;

            Array enumValues = typeof(ResourceType).GetEnumValues();
            this.Resources = new Resource[enumValues.Length];
            foreach (var enumValue in enumValues)
            {
                this.Resources[(int) enumValue] = new Resource((ResourceType) enumValue, Number.Zero);
            }

            this.TechnologyStatusById = new TechnologyStatus[Databases.Instance.TechnologyDefinitions.Length];
            foreach (var technology in Databases.Instance.TechnologyDefinitions)
            {
                this.TechnologyStatusById[technology.Id] = new TechnologyStatus()
                {
                    Definition = technology,
                    Status = ResearchStatus.Available,
                };
            }
        }

        public Player() : this(255)
        {
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

        public bool IsRecipeAvailable(Data.RecipeDefinition definition)
        {
            foreach (var technology in Databases.Instance.TechnologyDefinitions)
            {
                foreach (var unlock in technology.Unlocks)
                {
                    if (unlock == definition)
                    {
                        return this.TechnologyStatusById[technology.Id].Status == ResearchStatus.Done;
                    }
                }
            }

            return true;
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(this.ClientId);
            stream.Write(this.Resources);
            stream.Write(this.Factories);
            stream.Write(this.ConstructionQueue);
            stream.Write(this.TechnologyStatusById);
        }

        public void Deserialize(BinaryReader stream)
        {
            this.ClientId = stream.ReadByte();

            var deserializedResources = stream.ReadArray<Resource>();
            for (int index = 0; index < deserializedResources.Length; index++)
            {
                if (deserializedResources[index].Name == ResourceType.None)
                {
                    // Ignore deprecated resource.
                    continue;
                }

                this.Resources[(int)deserializedResources[index].Name].Amount = deserializedResources[index].Amount;
            }

            this.Factories = stream.ReadList<Factory>();
            for (int index = this.Factories.Count - 1; index >= 0; index--)
            {
                if (this.Factories[index].Definition == null)
                {
                    // Deprecated element. Remove it.
                    this.Factories.RemoveAt(index);
                }
            }

            this.ConstructionQueue = stream.ReadList<CraftTask>();
            for (int index = this.ConstructionQueue.Count - 1; index >= 0; index--)
            {
                if (this.ConstructionQueue[index].Definition == null)
                {
                    // Deprecated element. Remove it.
                    this.ConstructionQueue.RemoveAt(index);
                }
            }

            var technologyStatuses = stream.ReadArray<TechnologyStatus>();
            for (int index = 0; index < technologyStatuses.Length; index++)
            {
                var status = technologyStatuses[index];
                if (status.Definition != null)
                {
                    this.TechnologyStatusById[status.Definition.Id].Status = status.Status;
                }
            }
        }

        private void GenerateOrderData()
        {
            // Initialize order types array.
            var values = Enum.GetValues(typeof(OrderType));
            this.OrderById = new OrderData[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                this.OrderById[index] = new OrderData()
                {
                    Type = (OrderType)index,
                };
            }

            // Search passes on game interface.
            MethodInfo[] methodInfos = typeof(Player).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int index = 0; index < methodInfos.Length; index++)
            {
                try
                {
                    var serverPass = methodInfos[index].GetCustomAttribute<OrderServerPassAttribute>();
                    if (serverPass != null)
                    {
                        Debug.Assert(this.OrderById[(int)serverPass.OrderType].ServerPass == null);
                        this.OrderById[(int)serverPass.OrderType].Context = OrderContext.Player;
                        this.OrderById[(int)serverPass.OrderType].ServerPass = (OrderData.ServerPassDelegate)methodInfos[index].CreateDelegate(typeof(OrderData.ServerPassDelegate), this);
                        Debug.Assert(this.OrderById[(int)serverPass.OrderType].ServerPass != null);
                    }

                    var clientPass = methodInfos[index].GetCustomAttribute<OrderClientPassAttribute>();
                    if (clientPass != null)
                    {
                        Debug.Assert(this.OrderById[(int)clientPass.OrderType].ClientPass == null);
                        this.OrderById[(int)clientPass.OrderType].Context = OrderContext.Player;
                        this.OrderById[(int)clientPass.OrderType].ClientPass = (OrderData.ClientPassDelegate)methodInfos[index].CreateDelegate(typeof(OrderData.ClientPassDelegate), this);
                        Debug.Assert(this.OrderById[(int)clientPass.OrderType].ClientPass != null);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Invalid order pass: {methodInfos[index].Name}.\n{exception}");
                }
            }
        }
    }
}
