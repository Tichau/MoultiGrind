using System;
using System.Threading;
using Simulation.Network;
using UnityEngine;

namespace Simulation
{
    public class GameInstance
    {
        public readonly byte Id;
        public readonly Simulation.Game.Game Game;
        public readonly int DurationBetweenTwoTicks;

        private readonly Thread gameThread;

        private bool exit;

        /// <summary>
        /// Create a new instance of game server.
        /// </summary>
        /// <param name="durationBetweenTwoTicks">The duration between two game tick (in milliseconds).</param>
        /// <param name="timeElapsedPerTick">The game time elapsed per tick (in seconds).</param>
        public GameInstance(byte id, uint durationBetweenTwoTicks, ulong timeElapsedPerTick = 1UL)
        {
            Debug.Assert(timeElapsedPerTick >= 1);
            this.Id = id;
            this.DurationBetweenTwoTicks = (int)durationBetweenTwoTicks;
            this.Game = new Game.Game(timeElapsedPerTick);
            this.Game.Id = id;
            this.gameThread = new Thread(this.GameLoop);
        }

        public void Start()
        {
            this.gameThread.Start();

            Debug.Log($"Game {this.Id} started.");
        }

        public void Stop()
        {
            this.exit = true;

            while (this.gameThread.IsAlive)
            {
                Thread.Sleep(10);
            }

            Debug.Log($"Game {this.Id} stopped correctly.");
        }

        private void GameLoop()
        {
            while (!this.exit)
            {
                this.Game.Tick();

                Thread.Sleep(this.DurationBetweenTwoTicks);
            }
        }
    }
}
