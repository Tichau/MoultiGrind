using System.IO;
using Framework;

namespace Simulation.Network
{
    public struct GameInstanceSummary : ISerializable
    {
        public byte Id;
        public PlayerSlotSummary[] PlayerSlots;

        public GameInstanceSummary(GameInstance gameInstance)
        {
            this.Id = gameInstance.Id;
            this.PlayerSlots = new PlayerSlotSummary[gameInstance.Game.Players.Length];
            for (byte index = 0; index < gameInstance.Game.Players.Length; index++)
            {
                this.PlayerSlots[index] = new PlayerSlotSummary(index, gameInstance.Game.Players[index]);
            }
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(this.Id);
            stream.Write(this.PlayerSlots);
        }

        public void Deserialize(BinaryReader stream)
        {
            this.Id = stream.ReadByte();
            this.PlayerSlots = stream.ReadArray<PlayerSlotSummary>();
        }
    }
}
