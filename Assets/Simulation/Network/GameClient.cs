using System.IO;
using System.Threading;
using Framework.Network;
using UnityEngine;

namespace Simulation.Network
{
    public partial class GameClient : GameInterface
    {
        public Game Game;
        public byte PlayerId;

        private ulong durationBetweenTwoTicks;
        private ulong timeElapsedPerTick;

        private readonly Client client;

        private float lastTickDate = 0;

        public static GameClient Instance;

        private volatile uint nextOrderId = 1;

        public GameClient()
        {
            this.client = new Client();
            this.Writer = new BinaryWriter(this.WriteBuffer);
        }

        public Player ActivePlayer => this.Game.Players[this.PlayerId];

        public override void Start()
        {
            base.Start();

            this.client.Start();
            this.client.MessageReceived += this.OnMessageReceived;

            while (this.client.Id == Server.InvalidClientId)
            {
                Thread.Sleep(1);
            }

            Debug.Assert(GameClient.Instance == null);
            GameClient.Instance = this;
        }

        public override void Stop()
        {
            GameClient.Instance = null;

            if (this.client.State == InterfaceState.Started)
            {
                this.client.MessageReceived -= this.OnMessageReceived;
                this.client.Stop();
            }

            base.Stop();
        }

        public override void Dispose()
        {
            this.Stop();
        }

        public void Update()
        {
            if (this.Game == null)
            {
                return;
            }

            long timeElapsedSinceLastTick = Mathf.FloorToInt((Time.time - this.lastTickDate) * 1000);
            if (timeElapsedSinceLastTick > (long)this.durationBetweenTwoTicks)
            {
                this.Game.Tick();

                this.lastTickDate = Time.time;
            }
        }

        private void OnMessageReceived(MessageHeader header, BinaryReader buffer)
        {
            switch (header.Type)
            {
                case MessageType.GameOrder:
                    OrderHeader orderHeader = new OrderHeader(header, buffer);
                    if (orderHeader.Type == OrderType.Invalid)
                    {
                        Debug.LogWarning("Invalid order.");
                        return;
                    }

                    if (orderHeader.Status == OrderStatus.Validated)
                    {
                        // Get order data.
                        var orderData = this.OrderById[(int)orderHeader.Type];
                        if (orderData.Context == OrderContext.Invalid)
                        {
                            Debug.Assert(orderHeader.GameInstanceId != GameServer.InvalidGameId);
                            Debug.Assert(this.Game != null && this.Game.Id == orderHeader.GameInstanceId);

                            orderData = this.Game.OrderById[(int)orderHeader.Type];
                            if (orderData.Context == OrderContext.Invalid)
                            {
                                if (!this.Game.TryGetPlayer(orderHeader.ClientId, out Player player))
                                {
                                    Debug.LogWarning("Invalid order.");
                                    return;
                                }

                                orderData = player.OrderById[(int) orderHeader.Type];
                            }
                        }

                        Debug.Assert(orderData.Context != OrderContext.Invalid, $"No server pass context for order {orderData.Type}.");
                        Debug.Assert(orderData.ClientPass != null, $"No client pass for order {orderData.Type}.");
                        
                        orderData.ClientPass.Invoke(buffer);
                        orderHeader.Status = OrderStatus.Executed;
                    }

                    this.orders[orderHeader.Id] = orderHeader;
                    break;

                default:
                    break;
            }
        }

        internal OrderHeader WriteOrderHeader(OrderType type)
        {
            this.WriteBuffer.Position = 0;
            var id = this.nextOrderId++;
            var header = new OrderHeader(id, type, this.Game != null ? this.Game.Id : GameServer.InvalidGameId, this.client.Id);
            header.Write(this.Writer);
            return header;
        }

        internal OrderHeader WriteOrderHeader(OrderType type, byte gameInstanceId)
        {
            this.WriteBuffer.Position = 0;
            var id = this.nextOrderId++;
            var header = new OrderHeader(id, type, gameInstanceId, this.client.Id);
            header.Write(this.Writer);
            return header;
        }
        
        private async System.Threading.Tasks.Task<OrderHeader> ForOrderResponse(uint orderId)
        {
            while (this.orders[orderId].Status != OrderStatus.Executed &&
                   this.orders[orderId].Status != OrderStatus.Refused)
            {
                await System.Threading.Tasks.Task.Yield();
            }

            OrderHeader header = this.orders[orderId];
            switch (header.Status)
            {
                case OrderStatus.Refused:
                case OrderStatus.Executed:
                    break;

                default:
                    throw new System.Exception($"Invalid order status {header.Status}");
            }

            this.orders.Remove(header.Id);
            return header;
        }

        private void SendOrderFromWriteBuffer(OrderHeader header)
        {
            this.orders.Add(header.Id, header);
            this.client.SendMessage(this.WriteBuffer);
        }

        internal async System.Threading.Tasks.Task<OrderHeader> PostOrder(OrderHeader header)
        {
            this.SendOrderFromWriteBuffer(header);
            return await this.ForOrderResponse(header.Id);
        }
    }
}
