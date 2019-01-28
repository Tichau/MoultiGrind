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
    }
}
