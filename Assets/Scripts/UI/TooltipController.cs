using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TooltipController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform recipeTooltip = null;

        [SerializeField]
        private Text description = null;

        [SerializeField]
        private Text duration = null;

        [SerializeField]
        private TooltipResourceDefinitionList inputs = null;

        [SerializeField]
        private TooltipResourceDefinitionList outputs = null;

        private void Start()
        {
            Debug.Assert(this.recipeTooltip != null, "recipeTooltip should be set.");
            Debug.Assert(this.description != null, "description should be set.");
            Debug.Assert(this.duration != null, "duration should be set.");
            Debug.Assert(this.inputs != null, "inputs should be set.");
            Debug.Assert(this.outputs != null, "outputs should be set.");
        }

        private void Update()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
                position = Input.mousePosition,
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                var uiObject = result.gameObject;
                while (uiObject != this.gameObject)
                {
                    Vector2 position = new Vector2(Input.mousePosition.x, Input.mousePosition.y - Screen.height);
                    var tooltipInteractible = uiObject.GetComponent<TooltipInteractible>();
                    if (tooltipInteractible != null)
                    {
                        if (tooltipInteractible.Data is RecipeDefinition definition)
                        {
                            this.recipeTooltip.GetComponent<RectTransform>().anchoredPosition = position;
                            this.DisplayRecipeTooltip(definition);
                            this.recipeTooltip.gameObject.SetActive(true);
                            return;
                        }
                    }

                    uiObject = uiObject.transform.parent.gameObject;
                }
                
            }

            this.recipeTooltip.gameObject.SetActive(false);
        }

        private void DisplayResourceTooltip(ResourceType resourceType)
        {
            var resource = Game.Instance.Players[0].Resources[(int) resourceType];

            //TODO
        }

        private void DisplayRecipeTooltip(RecipeDefinition recipeDefinition)
        {
            const int titleHeight = 26;
            const int lineHeight = 20;
            const int margin = 5;
            int marginCount = 5;

            // Inputs
            this.inputs.ResourceDefinitions = recipeDefinition.Inputs;
            var inputTransform = this.inputs.GetComponent<RectTransform>();
            inputTransform.sizeDelta = new Vector2(inputTransform.sizeDelta.x, titleHeight + recipeDefinition.Inputs.Length * lineHeight);

            // Outputs
            this.outputs.ResourceDefinitions = recipeDefinition.Outputs;
            var outputTransform = this.outputs.GetComponent<RectTransform>();
            outputTransform.anchoredPosition = new Vector2(outputTransform.anchoredPosition.x, inputTransform.anchoredPosition.y - inputTransform.rect.height - margin);
            outputTransform.sizeDelta = new Vector2(outputTransform.sizeDelta.x, titleHeight + recipeDefinition.Outputs.Length * lineHeight);

            // Duration
            this.duration.text = recipeDefinition.Duration.ToString();
            var durationTransform = this.duration.transform.parent.GetComponent<RectTransform>();
            durationTransform.anchoredPosition = new Vector2(durationTransform.anchoredPosition.x, outputTransform.anchoredPosition.y - outputTransform.rect.height - margin);

            // Description.
            var descriptionTransform = this.description.GetComponent<RectTransform>();
            this.description.text = recipeDefinition.Description;
            if (string.IsNullOrEmpty(recipeDefinition.Description))
            {
                descriptionTransform.sizeDelta = new Vector2(descriptionTransform.sizeDelta.x, 0);
                marginCount--;
            }
            else
            {
                descriptionTransform.sizeDelta = new Vector2(descriptionTransform.sizeDelta.x, 44);
            }

            this.recipeTooltip.sizeDelta = new Vector2(this.recipeTooltip.sizeDelta.x, inputTransform.rect.height + outputTransform.rect.height + descriptionTransform.rect.height + titleHeight + marginCount * margin);
        }
    }
}
