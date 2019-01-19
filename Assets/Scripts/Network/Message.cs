namespace Network
{
    public struct Message
    {
        public ushort Size;
        public MessageType Type;
        public byte[] Data;

        public static Message Parse(byte[] buffer)
        {
            if (buffer.Length < 3)
            {
                // Invalid message.
                return new Message();
            }
            
            var message = new Message()
            {
                Size = (ushort)((buffer[0] << 8) + buffer[1]),
                Type = (MessageType)buffer[2],
            };

            UnityEngine.Debug.Assert(message.Size < buffer.Length - 3, "Big size not handled for now");

            if (message.Size > 0)
            {
                message.Data = new byte[message.Size];
                System.Array.Copy(buffer, 3, message.Data, 0, message.Size);
            }

            return message;
        }

        public static Message Ping()
        {
            return new Message()
            {
                Type = MessageType.Ping,
                Size = 0,
            };
        }

        public static Message Pong()
        {
            return new Message()
            {
                Type = MessageType.Pong,
                Size = 0,
            };
        }

        public static Message Text(string content)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(content);
            var message = new Message()
            {
                Type = MessageType.Text,
                Size = (ushort)bytes.Length,
                Data = bytes,
            };

            return message;
        }

        public void Write(byte[] buffer)
        {
            buffer[0] = (byte)(this.Size >> 8);
            buffer[1] = (byte)(this.Size & 0xFF);
            buffer[2] = (byte)this.Type;
            if (this.Data != null)
            {
                System.Array.Copy(this.Data, 0, buffer, 3, this.Size);
            }
        }

        public override string ToString()
        {
            string data = string.Empty;
            if (this.Size > 0)
            {
                data = System.Text.Encoding.ASCII.GetString(this.Data, 0, this.Size);
            }

            return $"[{this.Size}B] {this.Type} {data}";
        }
    }

    public enum MessageType : byte
    {
        Invalid,

        Ping,
        Pong,
        Text,
    }
}