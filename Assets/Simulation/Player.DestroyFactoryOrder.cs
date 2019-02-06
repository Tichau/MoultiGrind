namespace Simulation
{
    using System.Linq;

    using System.IO;
    using System.Threading.Tasks;

    using Framework;
    using Simulation.Network;

    public partial class Player
    {
        public bool CanDestroyFactory(RecipeDefinition definition)
        {
            return this.Factories.Any(factory => factory.Definition == definition && factory.Count > 0);
        }

        public async Task PostDestroyFactoryOrder(RecipeDefinition definition)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.DestroyFactory);
            WriteDestroyFactoryOrder(GameClient.Instance.Writer, definition.Id);

            header = await GameClient.Instance.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Craft recipe order failed.");
            }
        }

        private static void WriteDestroyFactoryOrder(BinaryWriter stream, uint recipeId)
        {
            stream.Write(recipeId);
        }

        private static void ReadDestroyFactoryOrder(BinaryReader stream, out uint recipeId)
        {
            recipeId = stream.ReadUInt32();
        }

        private void ApplyDestroyFactoryOrder(RecipeDefinition definition)
        {
            this.Resources[(int)ResourceType.AssemblingMachine1].Amount += new Number(1);

            var factory = this.Factories.Find(match => match.Definition == definition);
            factory.Count--;
        }

        [OrderServerPass(OrderType.DestroyFactory)]
        private OrderStatus DestroyFactoryServerPass(OrderHeader header, BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadDestroyFactoryOrder(dataFromClient, out var recipeId);

            RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];
           
            if (!this.CanDestroyFactory(definition))
            {
                return OrderStatus.Refused;
            }

            this.ApplyDestroyFactoryOrder(definition);
            
            WriteDestroyFactoryOrder(dataToClient, recipeId);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.DestroyFactory)]
        private void DestroyFactoryClientPass(OrderHeader header, BinaryReader dataFromServer)
        {
            ReadDestroyFactoryOrder(dataFromServer, out var recipeId);

            RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];

            this.ApplyDestroyFactoryOrder(definition);
        }
    }
}
