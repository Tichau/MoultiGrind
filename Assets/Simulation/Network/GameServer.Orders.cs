namespace Simulation.Network
{
    using System.IO;

    public partial class GameServer
    {
        [OrderServerPass(OrderType.CreateGame)]
        private OrderStatus CreateGameServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            dataFromClient.ReadCreateGameOrder(out var gameInstanceId, out var timeElapsedPerTick);

            gameInstanceId = this.nextGameId;
            this.nextGameId++;

            var gameInstance = new GameInstance(gameInstanceId, this.durationBetweenTwoTicks, timeElapsedPerTick);
            this.hostedGames.Add(gameInstance);
            gameInstance.Start();
            
            dataToClient.WriteCreateGameOrder(gameInstanceId, timeElapsedPerTick);

            return OrderStatus.Validated;
        }

        [OrderServerPass(OrderType.JoinGame)]
        private OrderStatus JoinGameServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            dataFromClient.ReadJoinGameOrder(out var gameInstanceId, out var clientId, out var timeElapsedPerTick, out var durationBetweenTwoTicks, out var playerId, out var gameSave);

            GameInstance game = null;
            for (int index = 0; index < this.hostedGames.Count; index++)
            {
                if (this.hostedGames[index].Id == gameInstanceId)
                {
                    game = this.hostedGames[index];
                }
            }

            if (game == null)
            {
                return OrderStatus.Refused;
            }

            playerId = game.Join(clientId);

            dataToClient.WriteJoinGameOrder(gameInstanceId, clientId, (ulong)game.Game.TimeElapsedPerTick, (ulong)game.DurationBetweenTwoTicks, playerId, game.Game);

            return OrderStatus.Validated;
        }
    }
}
