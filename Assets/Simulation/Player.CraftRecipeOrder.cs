namespace Simulation
{
    using System.IO;
    using System.Threading.Tasks;

    using UnityEngine;

    using Simulation.Network;

    public partial class Player
    {
        public bool CanCraftRecipe(RecipeDefinition definition)
        {
            bool resourcePrerequisites = true;
            foreach (var resource in definition.Inputs)
            {
                resourcePrerequisites &= resource.Amount <= this.Resources[(int)resource.Name].Amount;
            }

            return resourcePrerequisites;
        }

        public async Task PostCraftRecipeOrder(RecipeDefinition definition)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.CraftRecipe);
            WriteCraftRecipeOrder(GameClient.Instance.Writer, definition.Id);

            header = await GameClient.Instance.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Craft recipe order failed.");
            }
        }

        private static void WriteCraftRecipeOrder(BinaryWriter stream, uint recipeId)
        {
            stream.Write(recipeId);
        }

        private static void ReadCraftRecipeOrder(BinaryReader stream, out uint recipeId)
        {
            recipeId = stream.ReadUInt32();
        }

        private void ApplyCraftRecipeOrder(RecipeDefinition definition)
        {
            foreach (var resource in definition.Inputs)
            {
                this.Resources[(int)resource.Name].Amount -= resource.Amount;
            }

            this.ConstructionQueue.Add(new CraftTask(definition));
        }

        [OrderServerPass(OrderType.CraftRecipe)]
        private OrderStatus CraftRecipeServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadCraftRecipeOrder(dataFromClient, out var recipeId);

            RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];
           
            if (!this.CanCraftRecipe(definition))
            {
                return OrderStatus.Refused;
            }

            this.ApplyCraftRecipeOrder(definition);
            
            WriteCraftRecipeOrder(dataToClient, recipeId);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.CraftRecipe)]
        private void CraftRecipeClientPass(BinaryReader dataFromServer)
        {
            ReadCraftRecipeOrder(dataFromServer, out var recipeId);

            RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];

            this.ApplyCraftRecipeOrder(definition);
        }
    }
}
