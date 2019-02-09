using System;
using System.Collections.Generic;
using System.IO;
using Framework.Network;
using UnityEngine;

namespace Simulation.Network
{
    public partial class GameServer : GameInterface
    {
        public const byte InvalidGameId = 0;

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
                    
                    // Get order data.
                    var orderData = this.OrderById[(int) orderHeader.Type];
                    if (orderData.Context == OrderContext.Invalid)
                    {
                        Debug.Assert(orderHeader.GameInstanceId != InvalidGameId);

                        var gamesCount = this.hostedGames.Count;
                        Game game = null;
                        for (int index = 0; index < gamesCount; index++)
                        {
                            if (this.hostedGames[index].Id == orderHeader.GameInstanceId)
                            {
                                game = this.hostedGames[index].Game;
                                break;
                            }
                        }

                        orderData = game.OrderById[(int) orderHeader.Type];
                        if (orderData.Context == OrderContext.Invalid)
                        {
                            Debug.Assert(orderHeader.ClientId != Server.InvalidClientId);
                            if (!game.TryGetPlayer(orderHeader.ClientId, out Player player))
                            {
                                Debug.LogWarning("Invalid order.");
                                return;
                            }

                            orderData = player.OrderById[(int)orderHeader.Type];
                        }
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
