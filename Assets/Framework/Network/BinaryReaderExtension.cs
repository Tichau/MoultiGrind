using System.IO;

namespace Framework.Network
{
    public static class BinaryReaderExtension
    {
        public static MessageHeader ReadHeader(this BinaryReader stream)
        {
            if (stream.BaseStream.Length < 3)
            {
                return MessageHeader.Invalid;
            }

            var header = new MessageHeader()
            {
                Size = stream.ReadUInt16(),
                Type = (MessageType)stream.ReadByte(),
            };

            return header;
        }

        public static Message ReadMessage(this BinaryReader stream, MessageHeader header)
        {
            byte[] data = null;
            if (header.Size > 0)
            {
                data = stream.ReadBytes(header.Size);
            }

            return new Message(header, data);
        }

        public static void ReadConnectMessage(this BinaryReader stream, out byte clientId)
        {
            clientId = stream.ReadByte();
        }
    }
}
