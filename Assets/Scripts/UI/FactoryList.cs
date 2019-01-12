﻿namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class FactoryList : MonoBehaviour
    {
        private readonly List<RecipeButtons> recipeButtons = new List<RecipeButtons>();
        private readonly List<UnityEngine.UI.Text> factoryLines = new List<UnityEngine.UI.Text>();

        [SerializeField]
        private GameObject recipeLinePrefab;

        [SerializeField]
        private GameObject factoryPrefab;

        private void Start()
        {
            Debug.Assert(this.recipeLinePrefab != null, "Factory prefab should be set.");
            Debug.Assert(this.factoryPrefab != null, "Factory prefab should be set.");
        }

        private void Update()
        {
            // Buildable factories
            this.DisplayList(Game.Instance.RecipeDefinitions, this.recipeButtons, this.recipeLinePrefab, (def, ui) => ui.Definition = def);

            // Display factories
            var player = Game.Instance.Players[0];
            this.DisplayList(player.Factories, this.factoryLines, this.factoryPrefab, (factory, ui) => ui.text = factory.ToString());
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
                    var component = prefab.GetComponent<RectTransform>();
                    var gameObject = GameObject.Instantiate(prefab);
                    gameObject.transform.SetParent(this.transform, false);
                    var rectTransform = gameObject.GetComponent<RectTransform>();
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
