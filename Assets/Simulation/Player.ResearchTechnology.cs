﻿namespace Simulation
{
    using System.IO;
    using System.Threading.Tasks;

    using UnityEngine;

    using Simulation.Network;

    public partial class Player
    {
        public bool CanResearchTechnology(TechnologyDefinition definition)
        {
            if (this.TechnologyStatesByDefinition[definition] != ResearchStatus.Available)
            {
                return false;
            }

            bool resourcePrerequisites = true;
            foreach (var resource in definition.Costs)
            {
                resourcePrerequisites &= resource.Amount <= this.Resources[(int)resource.Name].Amount;
            }

            return resourcePrerequisites;
        }

        public async Task PostResearchTechnologyOrder(TechnologyDefinition definition)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.ResearchTechnology);
            WriteResearchTechnologyOrder(GameClient.Instance.Writer, definition.Id);

            header = await GameClient.Instance.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Craft recipe order failed.");
            }
        }

        private static void WriteResearchTechnologyOrder(BinaryWriter stream, uint technologyId)
        {
            stream.Write(technologyId);
        }

        private static void ReadResearchTechnologyOrder(BinaryReader stream, out uint recipeId)
        {
            recipeId = stream.ReadUInt32();
        }

        private void ApplyResearchTechnologyOrder(TechnologyDefinition definition)
        {
            if (!this.CanResearchTechnology(definition))
            {
                return;
            }

            foreach (var resource in definition.Costs)
            {
                this.Resources[(int)resource.Name].Amount -= resource.Amount;
            }

            this.TechnologyStatesByDefinition[definition] = ResearchStatus.Done;
        }

        [OrderServerPass(OrderType.ResearchTechnology)]
        private OrderStatus ResearchTechnologyServerPass(OrderHeader header, BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadResearchTechnologyOrder(dataFromClient, out var recipeId);

            TechnologyDefinition definition = Databases.Instance.TechnologyDefinitions[recipeId];
           
            if (!this.CanResearchTechnology(definition))
            {
                return OrderStatus.Refused;
            }

            this.ApplyResearchTechnologyOrder(definition);
            
            WriteResearchTechnologyOrder(dataToClient, recipeId);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.ResearchTechnology)]
        private void ResearchTechnologyClientPass(OrderHeader header, BinaryReader dataFromServer)
        {
            ReadResearchTechnologyOrder(dataFromServer, out var recipeId);

            TechnologyDefinition definition = Databases.Instance.TechnologyDefinitions[recipeId];

            this.ApplyResearchTechnologyOrder(definition);
        }
    }
}
