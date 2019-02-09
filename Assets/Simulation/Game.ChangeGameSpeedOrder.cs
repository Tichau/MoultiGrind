namespace Simulation
{
    using System.IO;
    using System.Threading.Tasks;

    using Framework;
    using Simulation.Network;

    public partial class Game
    {
        public bool CanChangeGameSpeed(ulong timeElapsedPerTick)
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public async Task PostChangeGameSpeedOrder(ulong timeElapsedPerTick)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.ChangeGameSpeed);
            WriteChangeGameSpeedOrder(GameClient.Instance.Writer, timeElapsedPerTick);

            header = await GameClient.Instance.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Craft recipe order failed.");
            }
        }

        private static void WriteChangeGameSpeedOrder(BinaryWriter stream, ulong timeElapsedPerTick)
        {
            stream.Write(timeElapsedPerTick);
        }

        private static void ReadChangeGameSpeedOrder(BinaryReader stream, out ulong timeElapsedPerTick)
        {
            timeElapsedPerTick = stream.ReadUInt64();
        }

        private void ApplyChangeGameSpeedOrder(ulong timeElapsedPerTick)
        {
            this.TimeElapsedPerTick = new Number(timeElapsedPerTick);
        }

        [OrderServerPass(OrderType.ChangeGameSpeed)]
        private OrderStatus ChangeGameSpeedServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadChangeGameSpeedOrder(dataFromClient, out var timeElapsedPerTick);

            if (!this.CanChangeGameSpeed(timeElapsedPerTick))
            {
                return OrderStatus.Refused;
            }

            this.ApplyChangeGameSpeedOrder(timeElapsedPerTick);

            WriteChangeGameSpeedOrder(dataToClient, timeElapsedPerTick);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.ChangeGameSpeed)]
        private void ChangeGameSpeedClientPass(BinaryReader dataFromServer)
        {
            ReadChangeGameSpeedOrder(dataFromServer, out var timeElapsedPerTick);

            this.ApplyChangeGameSpeedOrder(timeElapsedPerTick);
        }
    }
}
