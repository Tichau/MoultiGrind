using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Framework.Network;

namespace Framework
{
    public static class BinaryWriterExtension
    {
        public static void Write(this BinaryWriter stream, Number number)
        {
            number.Serialize(stream);
        }

        public static void Write<T>(this BinaryWriter stream, T[] array)
            where T : ISerializable
        {
            var isNull = array == null;
            stream.Write(isNull);
            if (isNull)
            {
                return;
            }

            stream.Write((ushort)array.Length);
            for (int index = 0; index < array.Length; index++)
            {
                array[index].Serialize(stream);
            }
        }

        public static void Write<T>(this BinaryWriter stream, List<T> list)
            where T : ISerializable
        {
            var isNull = list == null;
            stream.Write(isNull);
            if (isNull)
            {
                return;
            }

            stream.Write((ushort)list.Count);
            for (int index = 0; index < list.Count; index++)
            {
                list[index].Serialize(stream);
            }
        }

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
