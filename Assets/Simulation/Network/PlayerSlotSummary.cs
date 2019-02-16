using System.IO;
using Framework;

namespace Simulation.Network
{
    public struct PlayerSlotSummary : ISerializable
    {
        public byte Id;

        public PlayerSlotSummary(byte id, Player.Player player)
        {
            this.Id = id;
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(this.Id);
        }

        public void Deserialize(BinaryReader stream)
        {
            this.Id = stream.ReadByte();
        }
    }
}
