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
    }
}
