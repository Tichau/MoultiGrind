using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Simulation.Network
{
    public abstract class GameInterface : IDisposable
    {
        public const byte InvalidGameId = 0;
        public const byte InvalidPlayerId = 255;

        protected MemoryStream WriteBuffer = new MemoryStream();
        internal BinaryWriter Writer;

        protected OrderData[] OrderById;

#if UNITY_EDITOR
        public bool Test_TryGetOrderData(OrderHeader header, out OrderData orderData)
        {
            return this.TryGetOrderData(header, out orderData);
        }
#endif

        protected GameInterface()
        {
            this.GenerateOrderData();
        }

        public virtual void Start()
        {
            this.WriteBuffer = new MemoryStream();
            this.Writer = new BinaryWriter(this.WriteBuffer);
        }

        public virtual void Stop()
        {
            this.Writer?.Dispose();
            this.WriteBuffer?.Dispose();
        }

        public abstract void Dispose();

        protected abstract bool TryGetGame(byte gameId, out Game.Game game);

        protected bool TryGetOrderData(OrderHeader header, out OrderData orderData)
        {
            orderData = this.OrderById[(int)header.Type];
            if (orderData.Context != OrderContext.Invalid)
            {
                return true;
            }

            Debug.Assert(header.GameInstanceId != InvalidGameId);
            if (!this.TryGetGame(header.GameInstanceId, out var game))
            {
                return false;
            }

            orderData = game.OrderById[(int) header.Type];
            if (orderData.Context != OrderContext.Invalid)
            {
                return true;
            }

            Debug.Assert(header.ClientId != Framework.Network.Server.InvalidClientId);
            if (!game.TryGetPlayer(header.ClientId, out Simulation.Player.Player player))
            {
                return false;
            }

            orderData = player.OrderById[(int) header.Type];
            return true;
        }
        
        private void GenerateOrderData()
        {
            // Initialize order types array.
            var values = Enum.GetValues(typeof(OrderType));
            this.OrderById = new OrderData[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                this.OrderById[index] = new OrderData()
                {
                    Type = (OrderType) index,
                };
            }
            
            // Search passes on game interface.
            MethodInfo[] methodInfos = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int index = 0; index < methodInfos.Length; index++)
            {
                try
                {
                    var serverPass = methodInfos[index].GetCustomAttribute<OrderServerPassAttribute>();
                    if (serverPass != null)
                    {
                        Debug.Assert(this.OrderById[(int)serverPass.OrderType].ServerPass == null);
                        this.OrderById[(int)serverPass.OrderType].Context = OrderContext.Server;
                        this.OrderById[(int)serverPass.OrderType].ServerPass = (OrderData.ServerPassDelegate)methodInfos[index].CreateDelegate(typeof(OrderData.ServerPassDelegate), this);
                    }

                    var clientPass = methodInfos[index].GetCustomAttribute<OrderClientPassAttribute>();
                    if (clientPass != null)
                    {
                        Debug.Assert(this.OrderById[(int)clientPass.OrderType].ClientPass == null);
                        this.OrderById[(int)clientPass.OrderType].Context = OrderContext.Server;
                        this.OrderById[(int)clientPass.OrderType].ClientPass = (OrderData.ClientPassDelegate)methodInfos[index].CreateDelegate(typeof(OrderData.ClientPassDelegate), this);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Invalid order pass: {methodInfos[index].Name}.\n{exception}");
                }
            }
        }
    }

    public enum OrderStatus
    {
        None,

        Validated,
        Refused,
        Executed,
    }

    public struct OrderData
    {
        public OrderType Type;
        public OrderContext Context;

        public delegate OrderStatus ServerPassDelegate(BinaryReader dataFromClient, BinaryWriter dataToClient);
        public delegate void ClientPassDelegate(BinaryReader dataFromServer);

        public ServerPassDelegate ServerPass;
        public ClientPassDelegate ClientPass;
    }
}
