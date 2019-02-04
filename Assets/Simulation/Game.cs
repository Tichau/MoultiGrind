using System.IO;
using Framework;

namespace Simulation
{
    public class Game
    {
        public readonly Number TimeElapsedPerTick;
        public Player[] Players;

        public int TickIndex = 0;

        public Game(ulong timeElapsedPerTick = 1)
        {
            this.Players = new Player[0];
            this.TimeElapsedPerTick = new Number(timeElapsedPerTick);
        }

        public byte RegisterPlayer(byte clientId)
        {
            byte playerId = (byte)this.Players.Length;
            System.Array.Resize(ref this.Players, this.Players.Length + 1);
            this.Players[playerId] = new Player(clientId);
            return playerId;
        }
        
        public void Tick()
        {
            this.TickIndex++;

            foreach (var player in this.Players)
            {
                player.Tick(this.TimeElapsedPerTick);
            }
        }

        public void UnTick()
        {
            this.TickIndex--;

            foreach (var player in this.Players)
            {
                player.UnTick(this.TimeElapsedPerTick);
            }
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(this.TickIndex);
            stream.Write((byte)this.Players.Length);
            for (int index = 0; index < this.Players.Length; index++)
            {
                this.Players[index].Serialize(stream);
            }
        }

        public void Deserialize(BinaryReader stream)
        {
            this.TickIndex = stream.ReadInt32();
            var playerCount = stream.ReadByte();
            this.Players = new Player[playerCount];
            for (int index = 0; index < this.Players.Length; index++)
            {
                this.Players[index] = new Player();
                this.Players[index].Deserialize(stream);
            }
        }
    }
}
