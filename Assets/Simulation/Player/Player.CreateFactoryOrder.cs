using System.IO;
using System.Threading.Tasks;
using Framework;
using Simulation.Network;

namespace Simulation.Player
{
    public partial class Player
    {
        public bool CanCreateFactory(Simulation.Data.RecipeDefinition definition)
        {
            if (this.Resources[(int)ResourceType.AssemblingMachine1].Amount < new Number(1))
            {
                return false;
            }

            return true;
        }

        public async Task PostCreateFactoryOrder(Simulation.Data.RecipeDefinition definition)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.CreateFactory);
            WriteCreateFactoryOrder(GameClient.Instance.Writer, definition.Id);

            header = await GameClient.Instance.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Create factory order failed.");
            }
        }

        private static void WriteCreateFactoryOrder(BinaryWriter stream, uint recipeId)
        {
            stream.Write(recipeId);
        }

        private static void ReadCreateFactoryOrder(BinaryReader stream, out uint recipeId)
        {
            recipeId = stream.ReadUInt32();
        }

        private void ApplyCreateFactoryOrder(Simulation.Data.RecipeDefinition definition)
        {
            this.Resources[(int)ResourceType.AssemblingMachine1].Amount -= new Number(1);

            var factory = this.Factories.Find(match => match.Definition == definition);
            if (factory == null)
            {
                factory = new Factory(definition);
                this.Factories.Add(factory);
            }

            factory.Count++;
        }

        [OrderServerPass(OrderType.CreateFactory)]
        private OrderStatus CreateFactoryServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadCreateFactoryOrder(dataFromClient, out var recipeId);

            Simulation.Data.RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];
           
            if (!this.CanCreateFactory(definition))
            {
                return OrderStatus.Refused;
            }

            this.ApplyCreateFactoryOrder(definition);
            
            WriteCreateFactoryOrder(dataToClient, recipeId);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.CreateFactory)]
        private void CreateFactoryClientPass(BinaryReader dataFromServer)
        {
            ReadCreateFactoryOrder(dataFromServer, out var recipeId);

            Simulation.Data.RecipeDefinition definition = Databases.Instance.RecipeDefinitions[recipeId];

            this.ApplyCreateFactoryOrder(definition);
        }
    }
}
