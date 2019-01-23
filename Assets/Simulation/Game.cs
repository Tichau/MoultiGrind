using Framework;

namespace Simulation
{
    using System.Collections.Generic;

    public class Game
    {
        public readonly List<Player> Players = new List<Player>();
        public readonly Number TimeElapsedPerTick;

        public int TickIndex = 0;

        public Game(ulong timeElapsedPerTick = 1)
        {
            this.TimeElapsedPerTick = new Number(timeElapsedPerTick);
        }

        public uint RegisterPlayer()
        {
            uint playerId = (uint)this.Players.Count;
            this.Players.Add(new Player());
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
    }
}
