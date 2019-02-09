using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Framework;
using Simulation.Network;

namespace Simulation.Player
{
    public partial class Player
    {
        public bool CanDestroyFactory(Simulation.Data.RecipeDefinition definition)
        {
            return this.Factories.Any(factory => factory.Definition == definition && factory.Count > 0);
        }

        public async Task PostDestroyFactoryOrder(Simulation.Data.RecipeDefinition definition)
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

        private void ApplyDestroyFactoryOrder(Simulation.Data.RecipeDefinition definition)
        {
            this.Resources[(int)ResourceType.AssemblingMachine1].Amount += new Number(1);

            var factory = this.Factories.Find(match => match.Definition == definition);
            factory.Count--;
        }

        [OrderServerPass(OrderType.DestroyFactory)]
        private OrderStatus DestroyFactoryServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadDestroyFactoryOrder(dataFromClient, out var recipeId);

            Simulation.Data.RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];
           
            if (!this.CanDestroyFactory(definition))
            {
                return OrderStatus.Refused;
            }

            this.ApplyDestroyFactoryOrder(definition);
            
            WriteDestroyFactoryOrder(dataToClient, recipeId);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.DestroyFactory)]
        private void DestroyFactoryClientPass(BinaryReader dataFromServer)
        {
            ReadDestroyFactoryOrder(dataFromServer, out var recipeId);

            Simulation.Data.RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];

            this.ApplyDestroyFactoryOrder(definition);
        }
    }
}
