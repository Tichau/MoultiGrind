namespace Network
{
    public struct MessageHeader
    {
        public static readonly MessageHeader Invalid = new MessageHeader();

        public ushort Size;
        public MessageType Type;

        public static MessageHeader Parse(byte[] buffer)
        {
            if (buffer.Length < 3)
            {
                return MessageHeader.Invalid;
            }

            var header = new MessageHeader()
            {
                Size = (ushort)((buffer[0] << 8) + buffer[1]),
                Type = (MessageType)buffer[2],
            };

            return header;
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

        public void Write(byte[] buffer)
        {
            buffer[0] = (byte)(this.Size >> 8);
            buffer[1] = (byte)(this.Size & 0xFF);
            buffer[2] = (byte)this.Type;
        }

        public override string ToString()
        {
            string data = string.Empty;
            return $"[{this.Size}B] {this.Size}";
        }
    }

    public struct Message
    {
        public MessageHeader Header;
        public byte[] Data;

        public static Message Parse(byte[] buffer)
        {
            MessageHeader header = MessageHeader.Parse(buffer);

            if (header == MessageHeader.Invalid)
            {
                // Invalid message.
                return new Message();
            }
            
            var message = new Message()
            {
                Header = header,
            };

            UnityEngine.Debug.Assert(message.Header.Size < buffer.Length - 3, "Big size not handled for now");

            if (message.Header.Size > 0)
            {
                message.Data = new byte[message.Header.Size];
                System.Array.Copy(buffer, 3, message.Data, 0, message.Header.Size);
            }

            return message;
        }

        public static Message Ping()
        {
            return new Message()
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.Ping,
                    Size = 0,
                }
            };
        }

        public static Message Pong()
        {
            return new Message()
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.Pong,
                    Size = 0,
                }
            };
        }

        public static Message Text(string content)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(content);
            var message = new Message()
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.Text,
                    Size = (ushort)bytes.Length,
                },
                Data = bytes,
            };

            return message;
        }

        public void Write(byte[] buffer)
        {
            this.Header.Write(buffer);
            if (this.Data != null)
            {
                System.Array.Copy(this.Data, 0, buffer, 3, this.Header.Size);
            }
        }

        public override string ToString()
        {
            string data = string.Empty;
            if (this.Header.Size > 0)
            {
                data = System.Text.Encoding.ASCII.GetString(this.Data, 0, this.Header.Size);
            }

            return $"{this.Header} {data}";
        }
    }

    public enum MessageType : byte
    {
        Invalid,

        Ping,
        Pong,
        Text,
        Binary,
    }
}