using Gameplay;
using Network;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameServer
    {
        public Server Server;
        public Gameplay.Game Game;

        public float DurationBetweenTwoTicks;
        public ulong TimeElapsedPerTick;

        private float lastTickDate = 0;

        public void GameLoop()
        {
            if (Time.time - this.lastTickDate > this.DurationBetweenTwoTicks)
            {
                this.Game.Tick();

                this.lastTickDate = Time.time;
            }
        }
    }
}