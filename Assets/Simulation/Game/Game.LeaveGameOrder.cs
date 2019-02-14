using System.IO;
using System.Threading.Tasks;
using Framework;
using Framework.Network;
using Simulation.Network;
using UnityEngine;

namespace Simulation.Game
{
    public partial class Game
    {
        // There is no acknowledge for this task since the player is disconnected after leaving the game.
        internal void PostLeaveGameOrderFromClient(byte playerId)
        {
            var header = GameClient.Instance.WriteOrderHeader(OrderType.LeaveGame, this.Id);
            WriteLeaveGameOrder(GameClient.Instance.Writer, playerId);

            var order = GameClient.Instance.PostOrder(header);
        }

        internal void PostLeaveGameOrderFromServer(byte playerId)
        {
            var header = GameServer.Instance.WriteOrderHeader(OrderType.LeaveGame, this.Id);
            WriteLeaveGameOrder(GameServer.Instance.Writer, playerId);

            GameServer.Instance.PostServerOrder(ref header);

            if (header.Status != OrderStatus.Validated)
            {
                throw new System.Exception($"Leave game {this.Id} failed.");
            }
        }

        private static void WriteLeaveGameOrder(BinaryWriter stream, byte playerId)
        {
            stream.Write(playerId);
        }

        private static void ReadLeaveGameOrder(BinaryReader stream, out byte playerId)
        {
            playerId = stream.ReadByte();
        }

        private void ApplyLeaveGameOrder(byte playerId)
        {
            this.Players[playerId].ClientId = Server.InvalidClientId;
            
            Debug.Log($"[Game {this.Id}] Player {playerId} left the game.");
        }

        [OrderServerPass(OrderType.LeaveGame)]
        private OrderStatus LeaveGameServerPass(BinaryReader dataFromClient, BinaryWriter dataToClient)
        {
            ReadLeaveGameOrder(dataFromClient, out var playerId);
            
            this.ApplyLeaveGameOrder(playerId);

            WriteLeaveGameOrder(dataToClient, playerId);

            return OrderStatus.Validated;
        }

        [OrderClientPass(OrderType.LeaveGame)]
        private void LeaveGameClientPass(BinaryReader dataFromServer)
        {
            ReadLeaveGameOrder(dataFromServer, out var playerId);

            this.ApplyLeaveGameOrder(playerId);
        }
    }
}
