namespace Framework.Network
{
    public struct Message
    {
        public static readonly Message Invalid = new Message(MessageHeader.Invalid, null);
        public static readonly Message Ping = new Message(new MessageHeader(0, MessageType.Ping), null);
        public static readonly Message Pong = new Message(new MessageHeader(0, MessageType.Pong), null);

        public MessageHeader Header;
        public byte[] Data;

        public Message(MessageHeader header, byte[] data)
        {
            this.Header = header;
            this.Data = data;
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

        public override string ToString()
        {
            string data = string.Empty;
            if (this.Header.Size > 0)
            {
                data = System.Text.Encoding.ASCII.GetString(this.Data, 0, this.Header.Size);
            }

            return $"{this.Header}: {data}";
        }
    }
}
