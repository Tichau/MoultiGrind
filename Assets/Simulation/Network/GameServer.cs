using System;
using System.Collections.Generic;
using System.IO;
using Framework.Network;
using UnityEngine;

namespace Simulation.Network
{
    public partial class GameServer : GameInterface
    {
        public static GameServer Instance;

        private readonly List<GameInstance> hostedGames = new List<GameInstance>();
        private readonly Server server;
        
        private readonly uint durationBetweenTwoTicks;

        private byte nextGameId = 1;

        private BinaryReader writeBufferReader;

        /// <summary>
        /// Create a new instance of game server.
        /// </summary>
        /// <param name="durationBetweenTwoTicks">The duration between two game tick (in milliseconds).</param>
        public GameServer(System.Net.IPAddress address, int port, TimeSpan clientConnectionCheckTimeout, uint durationBetweenTwoTicks = 1000)
        {
            this.durationBetweenTwoTicks = durationBetweenTwoTicks;
            this.server = new Server(address, port, clientConnectionCheckTimeout);
        }

        public int GameCount => this.hostedGames.Count;

        public override void Start()
        {
            base.Start();

            this.writeBufferReader = new BinaryReader(this.WriteBuffer);

            this.server.Start();
            this.server.MessageReceived += this.OnMessageReceived;
            this.server.ClientDisconnected += this.OnClientDisconnected;

            Debug.Assert(GameServer.Instance == null);
            GameServer.Instance = this;
        }

        public override void Stop()
        {
            // Shutdown hosted games.
            for (int index = 0; index < this.hostedGames.Count; index++)
            {
                this.hostedGames[index].Stop();
            }

            this.writeBufferReader.Dispose();
            this.writeBufferReader = null;

            this.server.ClientDisconnected -= this.OnClientDisconnected;
            this.server.MessageReceived -= this.OnMessageReceived;
            this.server.Stop();

            GameServer.Instance = null;

            base.Stop();
        }

        public override void Dispose()
        {
            this.Stop();
        }

        internal OrderHeader WriteOrderHeader(OrderType type, byte gameInstanceId)
        {
            this.WriteBuffer.Position = 0;
            var header = new OrderHeader(0, type, gameInstanceId, Server.InvalidClientId);
            header.Write(this.Writer);
            return header;
        }

        internal void PostServerOrder(ref OrderHeader header)
        {
            this.WriteBuffer.Seek(OrderHeader.SizeOf, SeekOrigin.Begin);
            this.ProcessOrder(Server.InvalidClientId, this.writeBufferReader, ref header);
        }

        protected override bool TryGetGame(byte gameId, out Simulation.Game.Game game)
        {
            var gamesCount = this.hostedGames.Count;
            for (int index = 0; index < gamesCount; index++)
            {
                if (this.hostedGames[index].Id == gameId)
                {
                    game = this.hostedGames[index].Game;
                    return true;
                }
            }

            game = null;
            return false;
        }

        private void OnMessageReceived(byte clientId, MessageHeader header, BinaryReader buffer)
        {
            switch (header.Type)
            {
                case MessageType.GameOrder:
                    OrderHeader orderHeader = new OrderHeader(header, buffer);

                    Debug.Log($"[GameServer] {orderHeader.Type} order received from client {clientId}.");

                    this.ProcessOrder(clientId, buffer, ref orderHeader);
                    break;

                default:
                    break;
            }
        }

        private void ProcessOrder(byte clientId, BinaryReader buffer, ref OrderHeader orderHeader)
        {
            if (orderHeader.Type == OrderType.Invalid)
            {
                Debug.LogWarning("Invalid order.");
                return;
            }

            if (!this.TryGetOrderData(orderHeader, out var orderData))
            {
                Debug.LogWarning($"No order data found for order {orderHeader.Type}.");
                return;
            }

            Debug.Assert(orderData.Context != OrderContext.Invalid, $"No server pass context for order {orderData.Type}.");
            Debug.Assert(orderData.ServerPass != null, $"No server pass for order {orderData.Type}.");

            this.WriteBuffer.Position = 0;
            orderHeader.Write(this.Writer);
            orderHeader.Status = orderData.ServerPass.Invoke(buffer, this.Writer);
            Debug.Assert(this.WriteBuffer.Position - MessageHeader.SizeOf >= 0);
            orderHeader.BaseHeader.Size = (ushort) (this.WriteBuffer.Position - MessageHeader.SizeOf);

            // Update order size and status.
            this.WriteBuffer.Position = 0;
            this.Writer.Write(orderHeader.BaseHeader.Size);
            this.WriteBuffer.Position = OrderHeader.StatusPosition;
            this.Writer.Write((byte) orderHeader.Status);

            switch (orderData.Context)
            {
                case OrderContext.Invalid:
                    Debug.LogWarning("Invalid order context.");
                    break;

                case OrderContext.Server:
                    this.server.SendMessage(clientId, this.WriteBuffer);
                    break;

                case OrderContext.Game:
                case OrderContext.Player:
                    this.BroadcastOrder(orderHeader, this.WriteBuffer);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnClientDisconnected(byte clientId)
        {
            for (int index = 0; index < this.hostedGames.Count; index++)
            {
                var game = this.hostedGames[index].Game;
                for (byte playerId = 0; playerId < game.Players.Length; playerId++)
                {
                    var player = game.Players[playerId];
                    if (player.ClientId == clientId)
                    {
                        try
                        {
                            game.PostLeaveGameOrderFromServer(playerId);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }

                        return;
                    }
                }
            }
        }

        private void BroadcastOrder(OrderHeader header, Stream message)
        {
            GameInstance gameInstance = null;
            if (header.GameInstanceId > 0)
            {
                var gamesCount = this.hostedGames.Count;
                for (int index = 0; index < gamesCount; index++)
                {
                    if (this.hostedGames[index].Id == header.GameInstanceId)
                    {
                        gameInstance = this.hostedGames[index];
                        break;
                    }
                }
            }

            bool senderInGameInstance = false;
            if (gameInstance != null)
            {
                var playersCount = gameInstance.Game.Players.Length;
                for (int index = 0; index < playersCount; index++)
                {
                    var player = gameInstance.Game.Players[index];
                    if (player.ClientId == header.ClientId)
                    {
                        senderInGameInstance = true;
                        break;
                    }
                }
            }

            if (gameInstance != null)
            {
                var playersCount = gameInstance.Game.Players.Length;
                for (int index = 0; index < playersCount; index++)
                {
                    this.server.SendMessage(gameInstance.Game.Players[index].ClientId, message);   
                }
            }

            if (!senderInGameInstance)
            {
                this.server.SendMessage(header.ClientId, message);
            }
        }
    }
}
