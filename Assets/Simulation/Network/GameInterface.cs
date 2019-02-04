using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Simulation.Network
{
    public abstract class GameInterface : IDisposable
    {
        protected MemoryStream WriteBuffer = new MemoryStream();
        protected BinaryWriter Writer;

        protected OrderData[] OrderById;

#if UNITY_EDITOR
        public OrderData[] Test_OrderById => this.OrderById;
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

        private void GenerateOrderData()
        {
            var values = Enum.GetValues(typeof(OrderType));
            this.OrderById = new OrderData[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                this.OrderById[index] = new OrderData()
                {
                    Type = (OrderType) index,
                };
            }

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

        public delegate OrderStatus ServerPassDelegate(OrderHeader header, BinaryReader dataFromClient, BinaryWriter dataToClient);
        public delegate void ClientPassDelegate(OrderHeader header, BinaryReader dataFromServer);

        public ServerPassDelegate ServerPass;
        public ClientPassDelegate ClientPass;
    }
}
