namespace Simulation
{
    using System;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Technology", menuName = "Technology Definition", order = 1)]
    public class TechnologyDefinition : ScriptableObject, IDatabaseElement
    {
        [TextArea] public string Description;

        public ResourceDefinition[] Costs;

        public RecipeDefinition[] Unlocks;

        public uint Id { get; internal set; }
    }
}