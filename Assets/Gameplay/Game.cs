namespace Gameplay
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    public class Game : MonoBehaviour
    {
        public readonly List<Player> Players = new List<Player>();
        
        public float DurationBetweenTwoTicks = 1f;
        public ulong TimeElapsedPerTick = 1;

        private float lastTickDate = 0;

        private int tickIndex = 0;

        public static Game Instance { get; private set; }

        private void Awake()
        {
            Debug.Assert(Game.Instance == null, "Instance should be null before assignation.");
            Game.Instance = this;
        }

        private void Start()
        {
            this.Players.Add(new Player());
        }

        private void Update()
        {
            if (Time.time - this.lastTickDate > this.DurationBetweenTwoTicks)
            {
                foreach (var player in this.Players)
                {
                    player.Tick(new Number(TimeElapsedPerTick));
                }

                this.lastTickDate = Time.time;
            }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.A))
            {
                this.Players[0].Resources[(int) ResourceType.AssemblingMachine1].Amount += new Number(1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                this.Players[0].Resources[(int) ResourceType.SciencePack1].Amount += new Number(1);
            }
            else if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                this.TimeElapsedPerTick *= 2;
            }
            else if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (this.TimeElapsedPerTick > 1)
                {
                    this.TimeElapsedPerTick /= 2;
                }
            }
#endif
        }
    }
}