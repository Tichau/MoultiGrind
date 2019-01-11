using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Game : MonoBehaviour
{
    public readonly List<Player> Players = new List<Player>();

    public RecipeDefinition[] RecipeDefinitions;

    public float DurationBetweenTwoTicks = 1f;

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
                player.Tick(new Number(1)); // 1 second per tick
            }

            this.lastTickDate = Time.time;
        }
    }
}
