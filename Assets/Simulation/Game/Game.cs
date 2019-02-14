using System;
using System.IO;
using System.Reflection;
using Framework;
using Simulation.Network;
using UnityEngine;

namespace Simulation.Game
{
    public partial class Game : ISerializable
    {
        public Number TimeElapsedPerTick;
        public Player.Player[] Players;

        public int TickIndex = 0;

        internal byte Id;
        internal OrderData[] OrderById;

        public Game(ulong timeElapsedPerTick = 1)
        {
            this.Players = new Player.Player[0];
            this.TimeElapsedPerTick = new Number(timeElapsedPerTick);

            this.GenerateOrderData();
        }
        
        public void Tick()
        {
            this.TickIndex++;

            foreach (var player in this.Players)
            {
                player.Tick(this.TimeElapsedPerTick);
            }
        }

        public void UnTick()
        {
            this.TickIndex--;

            foreach (var player in this.Players)
            {
                player.UnTick(this.TimeElapsedPerTick);
            }
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(this.Id);
            stream.Write(this.TickIndex);
            stream.Write(this.TimeElapsedPerTick);
            stream.Write(this.Players);
        }

        public void Deserialize(BinaryReader stream)
        {
            this.Id = stream.ReadByte();
            this.TickIndex = stream.ReadInt32();
            this.TimeElapsedPerTick = stream.ReadNumber();
            this.Players = stream.ReadArray<Player.Player>();
        }

        public bool TryGetPlayer(byte clientId, out Player.Player player)
        {
            for (int index = 0; index < this.Players.Length; index++)
            {
                if (this.Players[index].ClientId == clientId)
                {
                    player = this.Players[index];
                    return true;
                }
            }

            player = null;
            return false;
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
                    Type = (OrderType)index,
                };
            }

            // Search passes on game interface.
            MethodInfo[] methodInfos = typeof(Game).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int index = 0; index < methodInfos.Length; index++)
            {
                try
                {
                    var serverPass = methodInfos[index].GetCustomAttribute<OrderServerPassAttribute>();
                    if (serverPass != null)
                    {
                        Debug.Assert(this.OrderById[(int)serverPass.OrderType].ServerPass == null);
                        this.OrderById[(int)serverPass.OrderType].Context = OrderContext.Game;
                        this.OrderById[(int)serverPass.OrderType].ServerPass = (OrderData.ServerPassDelegate)methodInfos[index].CreateDelegate(typeof(OrderData.ServerPassDelegate), this);
                        Debug.Assert(this.OrderById[(int)serverPass.OrderType].ServerPass != null);
                    }

                    var clientPass = methodInfos[index].GetCustomAttribute<OrderClientPassAttribute>();
                    if (clientPass != null)
                    {
                        Debug.Assert(this.OrderById[(int)clientPass.OrderType].ClientPass == null);
                        this.OrderById[(int)clientPass.OrderType].Context = OrderContext.Game;
                        this.OrderById[(int)clientPass.OrderType].ClientPass = (OrderData.ClientPassDelegate)methodInfos[index].CreateDelegate(typeof(OrderData.ClientPassDelegate), this);
                        Debug.Assert(this.OrderById[(int)clientPass.OrderType].ClientPass != null);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"Invalid order pass: {methodInfos[index].Name}.\n{exception}");
                }
            }
        }
    }
}
