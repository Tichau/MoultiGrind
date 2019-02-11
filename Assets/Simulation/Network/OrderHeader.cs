using System.IO;
using Framework;
using Framework.Network;

namespace Simulation.Network
{
    public struct OrderHeader
    {
        public const int SizeOf = MessageHeader.SizeOf + 8;
        public const int StatusPosition = MessageHeader.SizeOf + 5;

        public static readonly OrderHeader Invalid = new OrderHeader();

        public MessageHeader BaseHeader;
        public uint Id;
        public OrderType Type;
        public OrderStatus Status;
        public byte GameInstanceId;
        public byte ClientId;

        public OrderHeader(MessageHeader header, BinaryReader stream) : this()
        {
            this.BaseHeader = header;
            this.Read(stream);
        }

        public OrderHeader(uint id, OrderType type, byte gameInstanceId, byte clientId)
        {
            BaseHeader = new MessageHeader(0, MessageType.GameOrder);
            Id = id;
            Type = type;
            Status = OrderStatus.None;
            GameInstanceId = gameInstanceId;
            ClientId = clientId;
        }

        public void Write(BinaryWriter stream)
        {
            stream.WriteHeader(this.BaseHeader);
            stream.Write(this.Id);
            stream.Write((byte)this.Type);
            stream.Write((byte)this.Status);
            stream.Write(this.GameInstanceId);
            stream.Write(this.ClientId);
        }

        public void Read(BinaryReader stream)
        {
            this.Id = stream.ReadUInt32();
            this.Type = (OrderType)stream.ReadByte();
            this.Status = (OrderStatus)stream.ReadByte();
            this.GameInstanceId = stream.ReadByte();
            this.ClientId = stream.ReadByte();
        }

        public override string ToString()
        {
            var header = $"{this.Id}:{this.Type} [{this.Status}] from client {this.ClientId}";
            if (this.GameInstanceId > 0)
            {
                header = $"[{this.GameInstanceId}] {header}";
            }

            return header;
        }
    }
}
