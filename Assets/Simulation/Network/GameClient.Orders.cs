using System.IO;

namespace Simulation.Network
{
    public partial class GameClient
    {
        private System.Collections.Generic.Dictionary<uint, OrderHeader> orders = new System.Collections.Generic.Dictionary<uint, OrderHeader>();
        
        public async System.Threading.Tasks.Task<byte> PostCreateGameOrder(ulong timeElapsedPerTick = 1)
        {
            var header = this.WriteOrderHeader(OrderType.CreateGame);
            this.Writer.WriteCreateGameOrder(timeElapsedPerTick);

            header = await this.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Game creation failed.");
            }

            return header.GameInstanceId;
        }

        public async System.Threading.Tasks.Task PostJoinGameOrder(byte gameInstanceId)
        {
            var header = this.WriteOrderHeader(OrderType.JoinGame, gameInstanceId);
            this.Writer.WriteJoinGameOrder(0, 0, 0, null);

            header = await this.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Game creation failed.");
            }
        }

        [OrderClientPass(OrderType.CreateGame)]
        private void CreateGameClientPass(OrderHeader header, BinaryReader dataFromServer)
        {
            dataFromServer.ReadCreateGameOrder(out var timeElapsedPerTick);
        }

        [OrderClientPass(OrderType.JoinGame)]
        private void JoinGameClientPass(OrderHeader header, BinaryReader dataFromServer)
        {
            dataFromServer.ReadJoinGameOrder(out this.timeElapsedPerTick, out this.durationBetweenTwoTicks, out this.PlayerId, out this.Game);
        }
    }
}
