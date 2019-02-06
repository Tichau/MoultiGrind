namespace Simulation.Network
{
    using System.IO;
    using Simulation.Network;
    using UnityEngine;

    public partial class GameServer
    {
        [OrderServerPass(OrderType.CreateGame)]
        private OrderStatus CreateGameServerPass(OrderHeader header, BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            dataFromClient.ReadCreateGameOrder(out var timeElapsedPerTick);

            header.GameInstanceId = this.nextGameId;
            this.nextGameId++;

            var gameInstance = new GameInstance(header.GameInstanceId, this.durationBetweenTwoTicks, timeElapsedPerTick);
            this.hostedGames.Add(gameInstance);
            gameInstance.Start();

            Debug.Log($"[GameServer] Game {header.GameInstanceId} created.");

            // Specific case: For this order we modify a data stored in the header. 
            // TODO: Handle this in a better way.
            dataToClient.BaseStream.Position = 0;
            header.Write(dataToClient);
            dataToClient.WriteCreateGameOrder(timeElapsedPerTick);

            return OrderStatus.Validated;
        }

        [OrderServerPass(OrderType.JoinGame)]
        private OrderStatus JoinGameServerPass(OrderHeader header, BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            dataFromClient.ReadJoinGameOrder(out var timeElapsedPerTick, out var durationBetweenTwoTicks, out var playerId, out var gameSave);

            GameInstance game = null;
            for (int index = 0; index < this.hostedGames.Count; index++)
            {
                if (this.hostedGames[index].Id == header.GameInstanceId)
                {
                    game = this.hostedGames[index];
                }
            }

            if (game == null)
            {
                return OrderStatus.Refused;
            }

            playerId = game.Join(header.ClientId);

            dataToClient.WriteJoinGameOrder((ulong)game.Game.TimeElapsedPerTick, (ulong)game.DurationBetweenTwoTicks, playerId, game.Game);

            return OrderStatus.Validated;
        }
    }
}
