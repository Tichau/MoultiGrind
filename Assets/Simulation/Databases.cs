using Simulation.Data;

namespace Simulation
{
    using UnityEngine;

    public class Databases : MonoBehaviour
    {
        public Data.RecipeDefinition[] RecipeDefinitions;
        public Data.TechnologyDefinition[] TechnologyDefinitions;

        public static Databases Instance;
        
        private void Awake()
        {
            try
            {
                for (uint index = 0; index < this.RecipeDefinitions.Length; index++)
                {
                    this.RecipeDefinitions[index].Id = index;
                }

                for (uint index = 0; index < this.TechnologyDefinitions.Length; index++)
                {
                    this.TechnologyDefinitions[index].Id = index;
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }

            Debug.Assert(Databases.Instance == null, "Instance should be null before assignation.");
            Databases.Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }

        public bool TryGet<T>(uint id, out T definition) where T : IDatabaseElement
        {
            // TODO: Use reflection to avoid hardcode.
            if (typeof(T) == typeof(RecipeDefinition))
            {
                definition = (T)(this.RecipeDefinitions[id] as IDatabaseElement);
                return true;
            }

            if (typeof(T) == typeof(TechnologyDefinition))
            {
                definition = (T)(this.TechnologyDefinitions[id] as IDatabaseElement);
                return true;
            }

            Debug.LogWarning($"Unknown data type: {typeof(T).Name}.");
            definition = default;
            return false;
        }
    }
}
