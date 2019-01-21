namespace Gameplay
{
    using UnityEngine;

    public class Databases : MonoBehaviour
    {
        public RecipeDefinition[] RecipeDefinitions;
        public TechnologyDefinition[] TechnologyDefinitions;

        public static Databases Instance;

        private void Awake()
        {
            Debug.Assert(Databases.Instance == null, "Instance should be null before assignation.");
            Databases.Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
    }
}
