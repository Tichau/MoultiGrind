using System.IO;

namespace Framework.Network
{
    public static class BinaryWriterExtension
    {
        public static void WriteHeader(this BinaryWriter stream, MessageHeader header)
        {
            stream.Write(header.Size);
            stream.Write((byte)header.Type);
        }

        public static void WriteMessage(this BinaryWriter stream, Message message)
        {
            stream.WriteHeader(message.Header);
            if (message.Header.Size > 0)
            {
                stream.Write(message.Data);
            }
        }

        public static void WriteConnectMessage(this BinaryWriter stream, byte clientId)
        {
            stream.BaseStream.Position = 0;
            stream.WriteHeader(new MessageHeader(1, MessageType.Connect));
            stream.Write(clientId);
        }
    }
}
