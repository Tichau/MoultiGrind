using Gameplay;
using Network;
using UnityEngine;

public class GameClient
{
    public Client Client;
    public Gameplay.Game Game;

    public float DurationBetweenTwoTicks;
    public ulong TimeElapsedPerTick;

    private float lastTickDate = 0;

    public static GameClient Instance;

    public Player ActivePlayer => this.Game.Players[0];

    public void GameLoop()
    {
        if (Time.time - this.lastTickDate > this.DurationBetweenTwoTicks)
        {
            this.Game.Tick();

            this.lastTickDate = Time.time;
        }
    }
}
