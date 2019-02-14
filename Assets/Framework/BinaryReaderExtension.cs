using System.Collections.Generic;
using System.IO;
using Framework.Network;

namespace Framework
{
    public static class BinaryReaderExtension
    {
        public static Number ReadNumber(this BinaryReader stream)
        {
            Number number = new Number();
            number.Deserialize(stream);
            return number;
        }

        public static T[] ReadArray<T>(this BinaryReader stream)
            where T : ISerializable, new()
        {
            var count = stream.ReadUInt16();
            var array = new T[count];
            for (int index = 0; index < count; index++)
            {
                array[index] = new T();
                array[index].Deserialize(stream);
            }

            return array;
        }

        public static List<T> ReadList<T>(this BinaryReader stream)
            where T : ISerializable, new()
        {
            var count = stream.ReadUInt16();
            var list = new List<T>(count);
            for (int index = 0; index < count; index++)
            {
                var element = new T();
                element.Deserialize(stream);
                list.Add(element);
            }

            return list;
        }

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
