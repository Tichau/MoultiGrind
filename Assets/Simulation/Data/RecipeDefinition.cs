﻿using System;
using Framework;
using UnityEngine;

namespace Simulation.Data
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Recipe Definition", order = 1)]
    public class RecipeDefinition : ScriptableObject, IDatabaseElement
    {
        [TextArea] public string Description;

        public ResourceDefinition[] Inputs;
        public ResourceDefinition[] Outputs;

        [SerializeField] private long fixedPointDuration;

        public Number Duration => Number.FromFixedPoint(this.fixedPointDuration);

        public uint Id { get; internal set; }
    }

    [Serializable]
    public struct ResourceDefinition
    {
        public ResourceType Name;

        [SerializeField] private long fixedPointAmount;

        public Number Amount => Number.FromFixedPoint(this.fixedPointAmount);
    }
}
