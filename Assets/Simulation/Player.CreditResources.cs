using Framework;

namespace Simulation
{
    using System.IO;
    using System.Threading.Tasks;

    using UnityEngine;

    using Simulation.Network;

    public partial class Player
    {
        public bool CanCreditResources(ResourceType resource, long amount)
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public async Task PostCreditResourcesOrder(ResourceType resource, long amount)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.CreditResources);
            WriteCreditResourcesOrder(GameClient.Instance.Writer, resource, amount);

            header = await GameClient.Instance.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Craft recipe order failed.");
            }
        }

        private static void WriteCreditResourcesOrder(BinaryWriter stream, ResourceType resource, long amount)
        {
            stream.Write((byte)resource);
            stream.Write(amount);
        }

        private static void ReadCreditResourcesOrder(BinaryReader stream, out ResourceType resource, out long amount)
        {
            resource = (ResourceType)stream.ReadByte();
            amount = stream.ReadInt64();
        }

        private void ApplyCreditResourcesOrder(ResourceType resource, long amount)
        {
            this.Resources[(int)resource].Amount += new Number(amount);
        }

        [OrderServerPass(OrderType.CreditResources)]
        private OrderStatus CreditResourcesServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadCreditResourcesOrder(dataFromClient, out var resource, out var amount);
            
            if (!this.CanCreditResources(resource, amount))
            {
                return OrderStatus.Refused;
            }

            this.ApplyCreditResourcesOrder(resource, amount);
            
            WriteCreditResourcesOrder(dataToClient, resource, amount);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.CreditResources)]
        private void CreditResourcesClientPass(BinaryReader dataFromServer)
        {
            ReadCreditResourcesOrder(dataFromServer, out var resource, out var amount);

            this.ApplyCreditResourcesOrder(resource, amount);
        }
    }
}
