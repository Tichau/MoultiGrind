using System.IO;

namespace Framework
{
    public interface ISerializable
    {
        void Serialize(BinaryWriter stream);

        void Deserialize(BinaryReader stream);
    }
}
