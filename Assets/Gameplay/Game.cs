namespace Gameplay
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    public class Game
    {
        public readonly List<Player> Players = new List<Player>();
        
        public float DurationBetweenTwoTicks;
        public ulong TimeElapsedPerTick;

        private float lastTickDate = 0;

        private int tickIndex = 0;

        public Game(float durationBetweenTwoTicks = 1f, ulong timeElapsedPerTick = 1)
        {
            this.DurationBetweenTwoTicks = durationBetweenTwoTicks;
            this.TimeElapsedPerTick = timeElapsedPerTick;

            this.Players.Add(new Player());
        }
        
        public void Tick()
        {
            foreach (var player in this.Players)
            {
                player.Tick(new Number(TimeElapsedPerTick));
            }

            this.tickIndex++;

//#if UNITY_EDITOR
//            if (Input.GetKeyDown(KeyCode.A))
//            {
//                this.Players[0].Resources[(int) ResourceType.AssemblingMachine1].Amount += new Number(1);
//            }
//            else if (Input.GetKeyDown(KeyCode.S))
//            {
//                this.Players[0].Resources[(int) ResourceType.SciencePack1].Amount += new Number(1);
//            }
//            else if (Input.GetKeyDown(KeyCode.KeypadPlus))
//            {
//                this.TimeElapsedPerTick *= 2;
//            }
//            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
//            {
//                if (this.TimeElapsedPerTick > 1)
//                {
//                    this.TimeElapsedPerTick /= 2;
//                }
//            }
//#endif
        }
    }
}