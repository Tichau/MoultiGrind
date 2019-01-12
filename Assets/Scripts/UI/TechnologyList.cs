namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TechnologyList : MonoBehaviour
    {
        private readonly List<TechnologyButton> technologyButtons = new List<TechnologyButton>();

        [SerializeField]
        private GameObject technologyPrefab;

        private Predicate<KeyValuePair<TechnologyDefinition, ResearchStatus>> displayPredicate = def => def.Value == ResearchStatus.Available || def.Value == ResearchStatus.InProgress;

        private void Start()
        {
            Debug.Assert(this.technologyPrefab != null, "Technology prefab should be set.");
        }

        private void Update()
        {
            // Display factories
            var player = Game.Instance.Players[0];
            this.DisplayList(player.TechnologyStatesByDefinition, this.technologyButtons, this.technologyPrefab, this.displayPredicate, (def, ui) => ui.Definition = def.Key);
        }

        private void DisplayList<TGame, TUI>(IEnumerable<TGame> gameElements, List<TUI> uiElements, GameObject prefab, Predicate<TGame> displayPredicate, Action<TGame, TUI> updateElement)
            where TUI : MonoBehaviour
        {
            int index = 0;
            foreach (var element in gameElements)
            {
                if (displayPredicate != null && !displayPredicate.Invoke(element))
                {
                    continue;
                }

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
