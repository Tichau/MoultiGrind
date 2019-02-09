using UnityEngine;

namespace Simulation.Data
{
    [CreateAssetMenu(fileName = "Technology", menuName = "Technology Definition", order = 1)]
    public class TechnologyDefinition : ScriptableObject, IDatabaseElement
    {
        [TextArea] public string Description;

        public Simulation.Data.ResourceDefinition[] Costs;

        public Simulation.Data.RecipeDefinition[] Unlocks;

        public uint Id { get; internal set; }
    }
}
