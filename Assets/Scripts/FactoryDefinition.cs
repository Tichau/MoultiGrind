namespace DefaultNamespace
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "Factory", menuName = "Factory Definition", order = 1)]
    public class FactoryDefinition : ScriptableObject
    {
        public string Name = "New Factory";

        public Resource[] Inputs;
        public Resource[] Outputs;
    }
}