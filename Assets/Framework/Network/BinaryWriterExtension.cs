using System.Diagnostics;
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

        public static void WriteTextMessage(this BinaryWriter stream, string text)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(text);
            Debug.Assert(bytes.Length < ushort.MaxValue);
            stream.BaseStream.Position = 0;
            stream.WriteHeader(new MessageHeader((ushort)bytes.Length, MessageType.Text));
            stream.Write(bytes);
        }

        public static void WriteConnectMessage(this BinaryWriter stream, byte clientId)
        {
            stream.BaseStream.Position = 0;
            stream.WriteHeader(new MessageHeader(1, MessageType.Connect));
            stream.Write(clientId);
        }
    }
}
