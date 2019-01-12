namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        private readonly List<UnityEngine.UI.Text> craftTasks = new List<UnityEngine.UI.Text>();

        [SerializeField]
        private GameObject craftTaskPrefab;

        private void Start()
        {
            Debug.Assert(this.craftTaskPrefab != null, "Craft task prefab should be set.");
        }

        private void Update()
        {
            // Display factories
            var player = Game.Instance.Players[0];
            this.DisplayList(player.ConstructionQueue, this.craftTasks, this.craftTaskPrefab, (craftTask, ui) => ui.text = craftTask.ToString());
        }

        private void DisplayList<TGame, TUI>(IEnumerable<TGame> gameElements, List<TUI> uiElements, GameObject prefab, Action<TGame, TUI> updateElement)
            where TUI : MonoBehaviour
        {
            int index = 0;
            foreach (var element in gameElements)
            {
                TUI line = default(TUI);
                if (index < uiElements.Count)
                {
                    line = uiElements[index];
                }
                else
                {
                    var gameObject = GameObject.Instantiate(prefab);
                    gameObject.transform.SetParent(this.transform, false);
                    line = gameObject.GetComponent<TUI>();
                    uiElements.Add(line);
                }

                updateElement.Invoke(element, line);
                index++;
            }

            for (int remainingIndex = uiElements.Count - 1; remainingIndex >= index; remainingIndex--)
            {
                GameObject.Destroy(uiElements[remainingIndex].gameObject);
                uiElements.RemoveAt(remainingIndex);
            }
        }
    }
}
