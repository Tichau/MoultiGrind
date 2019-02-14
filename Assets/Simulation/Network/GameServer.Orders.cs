using Framework.Network;
using UnityEngine;

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

            GameInstance instance = null;
            for (int index = 0; index < this.hostedGames.Count; index++)
            {
                if (this.hostedGames[index].Id == gameInstanceId)
                {
                    instance = this.hostedGames[index];
                }
            }

            if (instance == null)
            {
                // Game instance not found.
                Debug.LogWarning($"Can't retrieve game {gameInstanceId}.");
                return OrderStatus.Refused;
            }

            if (playerId != GameServer.InvalidPlayerId)
            {
                // A specific player is requested.

                if (playerId >= instance.Game.Players.Length)
                {
                    // Player doesn't exist.
                    Debug.LogWarning($"Player {playerId} does not exist in game {gameInstanceId}.");
                    return OrderStatus.Refused;
                }

                var player = instance.Game.Players[playerId];
                if (player.ClientId != Server.InvalidClientId)
                {
                    // Player is already controlled by another client.
                    Debug.LogWarning($"Player {playerId} is already controlled by another client in game {gameInstanceId}.");
                    return OrderStatus.Refused;
                }

                player.ClientId = clientId;
            }
            else
            {
                // Create a new player.
                playerId = (byte)instance.Game.Players.Length;
                System.Array.Resize(ref instance.Game.Players, instance.Game.Players.Length + 1);
                instance.Game.Players[playerId] = new Player.Player(clientId);
            }

            Debug.Log($"Client {clientId} join game {gameInstanceId} on player {playerId} slot.");

            dataToClient.WriteJoinGameOrder(gameInstanceId, clientId, (ulong)instance.Game.TimeElapsedPerTick, (ulong)instance.DurationBetweenTwoTicks, playerId, instance.Game);

            return OrderStatus.Validated;
        }
    }
}
