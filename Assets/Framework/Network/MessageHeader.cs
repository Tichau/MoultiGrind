namespace Framework.Network
{
    public struct MessageHeader
    {
        public const int SizeOf = 3;

        public static readonly MessageHeader Invalid = new MessageHeader();

        public ushort Size;
        public MessageType Type;
        
        public MessageHeader(ushort size, MessageType type)
        {
            Size = size;
            Type = type;
        }

        public static bool operator ==(MessageHeader left, MessageHeader right)
        {
            return left.Type == right.Type && left.Size == right.Size;
        }

        public static bool operator !=(MessageHeader left, MessageHeader right)
        {
            return left.Type != right.Type || left.Size != right.Size;
        }

        public bool Equals(MessageHeader other)
        {
            return Size == other.Size && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MessageHeader other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Size.GetHashCode() * 397) ^ (int)Type;
            }
        }

        public override string ToString()
        {
            string data = string.Empty;
            return $"[{this.Size}B] {this.Type}";
        }
    }
}
