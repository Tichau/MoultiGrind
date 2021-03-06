﻿namespace Framework.Network
{
    using System.IO;

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

        public static void ReadTextMessage(this BinaryReader stream, MessageHeader header, out string text)
        {
            var bytes = stream.ReadBytes(header.Size);
            text = System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static void ReadConnectMessage(this BinaryReader stream, out byte clientId)
        {
            clientId = stream.ReadByte();
        }
    }
}
