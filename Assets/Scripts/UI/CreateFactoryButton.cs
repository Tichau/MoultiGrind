namespace UI
{
    using System.Collections;
    using System.Collections.Generic;

    using DefaultNamespace;

    using UnityEngine;
    using UnityEngine.UI;

    public class CreateFactoryButton : MonoBehaviour
    {
        private FactoryDefinition definition;

        public FactoryDefinition Definition
        {
            get
            {
                return this.definition;
            }

            set
            {
                this.definition = value;
                this.GetComponentInChildren<Text>().text = this.definition.Name;
            }
        }

        public void CreateFactory()
        {
            Game.Instance.Players[0].CreateFactory(this.Definition);
        }
    }
}
