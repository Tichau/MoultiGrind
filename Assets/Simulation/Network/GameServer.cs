using System;
using System.Collections.Generic;
using System.IO;
using Framework.Network;
using UnityEngine;

namespace Simulation.Network
{
    public partial class GameServer : GameInterface
    {
        private readonly List<GameInstance> hostedGames = new List<GameInstance>();
        private readonly Server server;
        
        private readonly uint durationBetweenTwoTicks;

        private byte nextGameId = 1;

        /// <summary>
        /// Create a new instance of game server.
        /// </summary>
        /// <param name="durationBetweenTwoTicks">The duration between two game tick (in milliseconds).</param>
        public GameServer(uint durationBetweenTwoTicks = 1000)
        {
            this.durationBetweenTwoTicks = durationBetweenTwoTicks;
            this.server = new Server();
        }

        public int GameCount => this.hostedGames.Count;

        public override void Start()
        {
            base.Start();

            this.server.Start();
            this.server.MessageReceived += this.OnMessageReceived;
        }

        public override void Stop()
        {
            // Shutdown hosted games.
            for (int index = 0; index < this.hostedGames.Count; index++)
            {
                this.hostedGames[index].Stop();
            }

            this.server.MessageReceived -= this.OnMessageReceived;
            this.server.Stop();

            base.Stop();
        }

        public override void Dispose()
        {
            this.Stop();
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
                    orderHeader.BaseHeader.Size = (ushort)(this.WriteBuffer.Position - MessageHeader.SizeOf);

                    // Update order size and status.
                    this.WriteBuffer.Position = 0;
                    this.Writer.Write(orderHeader.BaseHeader.Size);
                    this.WriteBuffer.Position = OrderHeader.StatusPosition;
                    this.Writer.Write((byte)orderHeader.Status);

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

                    break;

                default:
                    break;
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
