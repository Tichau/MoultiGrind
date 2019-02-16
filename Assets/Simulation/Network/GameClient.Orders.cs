using System.IO;

namespace Simulation.Network
{
    public partial class GameClient
    {
        public delegate void GameCreatedDelegate(byte gameInstanceId);

        public event GameCreatedDelegate GameCreated;

        private System.Collections.Generic.Dictionary<uint, OrderHeader> orders = new System.Collections.Generic.Dictionary<uint, OrderHeader>();

        private GameInstanceSummary[] gameServerInstances;

        public async System.Threading.Tasks.Task<byte> PostCreateGameOrder(ulong timeElapsedPerTick = 1)
        {
            byte gameInstanceId = 0;

            void OnGameCreated(byte id) => gameInstanceId = id;
            this.GameCreated += OnGameCreated;

            var header = this.WriteOrderHeader(OrderType.CreateGame);
            this.Writer.WriteCreateGameOrder(0, timeElapsedPerTick);

            header = await this.PostOrder(header);

            this.GameCreated -= OnGameCreated;

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception("Game creation failed.");
            }

            if (gameInstanceId == 0)
            {
                throw new System.Exception("Invalid game instance id.");
            }

            return gameInstanceId;
        }

        public async System.Threading.Tasks.Task PostJoinGameOrder(byte gameInstanceId, byte playerId = GameClient.InvalidPlayerId)
        {
            var header = this.WriteOrderHeader(OrderType.JoinGame);
            this.Writer.WriteJoinGameOrder(gameInstanceId, 0, 0, 0, playerId, null);

            header = await this.PostOrder(header);

            if (header.Status != OrderStatus.Executed)
            {
                throw new System.Exception($"Join game {gameInstanceId} failed.");
            }
        }

        public async System.Threading.Tasks.Task<GameInstanceSummary[]> PostListGamesOrder()
        {
            var header = this.WriteOrderHeader(OrderType.ListGames);
            this.Writer.WriteListGamesOrder(null);

            header = await this.PostOrder(header);
            
            if (header.Status != OrderStatus.Executed)
            {
                this.gameServerInstances = null;
                throw new System.Exception("Game creation failed.");
            }

            return this.gameServerInstances;
        }

        [OrderClientPass(OrderType.CreateGame)]
        private void CreateGameClientPass(BinaryReader dataFromServer)
        {
            dataFromServer.ReadCreateGameOrder(out var gameInstanceId, out var timeElapsedPerTick);

            this.GameCreated?.Invoke(gameInstanceId);
        }

        [OrderClientPass(OrderType.JoinGame)]
        private void JoinGameClientPass(BinaryReader dataFromServer)
        {
            dataFromServer.ReadJoinGameOrder(out var gameInstanceId, out var clientId, out this.timeElapsedPerTick, out this.durationBetweenTwoTicks, out this.PlayerId, out this.Game);
        }

        [OrderClientPass(OrderType.ListGames)]
        private void ListGamesClientPass(BinaryReader dataFromServer)
        {
            dataFromServer.ReadListGamesOrder(out this.gameServerInstances);
        }
    }
}
