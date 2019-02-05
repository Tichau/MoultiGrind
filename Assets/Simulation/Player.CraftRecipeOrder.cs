namespace Simulation
{
    using System.IO;
    using System.Threading.Tasks;

    using UnityEngine;

    using Simulation.Network;

    public partial class Player
    {
        public async Task PostCraftRecipeOrder(RecipeDefinition definition)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.CraftRecipe);
            GameClient.Instance.Writer.WriteCraftRecipeOrder(definition.Id);

            header = await GameClient.Instance.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Craft recipe order failed.");
            }
        }

        public bool CanCraftRecipe(RecipeDefinition definition)
        {
            bool resourcePrerequisites = true;
            foreach (var resource in definition.Inputs)
            {
                resourcePrerequisites &= resource.Amount <= this.Resources[(int)resource.Name].Amount;
            }

            return resourcePrerequisites;
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
        private OrderStatus CraftRecipeServerPass(OrderHeader header, BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            dataFromClient.ReadCraftRecipeOrder(out var recipeId);

            RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];
           
            if (!this.CanCraftRecipe(definition))
            {
                return OrderStatus.Refused;
            }

            this.ApplyCraftRecipeOrder(definition);
            
            header.Write(dataToClient);
            dataToClient.WriteCraftRecipeOrder(recipeId);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.CraftRecipe)]
        private void CraftRecipeClientPass(OrderHeader header, BinaryReader dataFromServer)
        {
            dataFromServer.ReadCraftRecipeOrder(out var recipeId);

            RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];

            this.ApplyCraftRecipeOrder(definition);
        }
    }
}
