using System.Linq;
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
        private Text recipeDescription = null;

        [SerializeField]
        private Text duration = null;

        [SerializeField]
        private TooltipResourceDefinitionList inputs = null;

        [SerializeField]
        private TooltipResourceDefinitionList outputs = null;


        [SerializeField]
        private RectTransform technologyTooltip = null;

        [SerializeField]
        private Text technologyDescription = null;

        [SerializeField]
        private TooltipResourceDefinitionList costs = null;

        [SerializeField]
        private TooltipResourceDefinitionList unlocks = null;

        private void Start()
        {
            Debug.Assert(this.recipeTooltip != null, "recipeTooltip should be set.");
            Debug.Assert(this.recipeDescription != null, "recipeDescription should be set.");
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
                        if (tooltipInteractible.Data is RecipeDefinition recipeDefinition)
                        {
                            this.recipeTooltip.GetComponent<RectTransform>().anchoredPosition = position;
                            this.DisplayRecipeTooltip(recipeDefinition);
                            this.recipeTooltip.gameObject.SetActive(true);
                            return;
                        }

                        if (tooltipInteractible.Data is TechnologyDefinition technologyDefinition)
                        {
                            this.technologyTooltip.GetComponent<RectTransform>().anchoredPosition = position;
                            this.DisplayTechnologyTooltip(technologyDefinition);
                            this.technologyTooltip.gameObject.SetActive(true);
                            return;
                        }
                    }

                    uiObject = uiObject.transform.parent.gameObject;
                }
                
            }

            this.recipeTooltip.gameObject.SetActive(false);
            this.technologyTooltip.gameObject.SetActive(false);
        }

        private void DisplayTechnologyTooltip(TechnologyDefinition technologyDefinition)
        {
            const int titleHeight = 26;
            const int lineHeight = 20;
            const int margin = 5;
            int marginCount = 4;

            // Inputs
            this.costs.Definitions = technologyDefinition.Costs.Cast<object>();
            var costsTransform = this.costs.GetComponent<RectTransform>();
            costsTransform.sizeDelta = new Vector2(costsTransform.sizeDelta.x, titleHeight + technologyDefinition.Costs.Length * lineHeight);

            // Unlocks
            this.unlocks.Definitions = technologyDefinition.Unlocks;
            var unlocksTransform = this.outputs.GetComponent<RectTransform>();
            unlocksTransform.anchoredPosition = new Vector2(unlocksTransform.anchoredPosition.x, costsTransform.anchoredPosition.y - costsTransform.rect.height - margin);
            unlocksTransform.sizeDelta = new Vector2(unlocksTransform.sizeDelta.x, titleHeight + technologyDefinition.Unlocks.Length * lineHeight);

            // Description.
            var descriptionTransform = this.technologyDescription.GetComponent<RectTransform>();
            this.technologyDescription.text = technologyDefinition.Description;
            if (string.IsNullOrEmpty(technologyDefinition.Description))
            {
                descriptionTransform.sizeDelta = new Vector2(descriptionTransform.sizeDelta.x, 0);
                marginCount--;
            }
            else
            {
                descriptionTransform.sizeDelta = new Vector2(descriptionTransform.sizeDelta.x, 44);
            }

            this.technologyTooltip.sizeDelta = new Vector2(this.technologyTooltip.sizeDelta.x, costsTransform.rect.height + unlocksTransform.rect.height + descriptionTransform.rect.height + marginCount * margin);
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
            this.inputs.Definitions = recipeDefinition.Inputs.Cast<object>();
            var inputTransform = this.inputs.GetComponent<RectTransform>();
            inputTransform.sizeDelta = new Vector2(inputTransform.sizeDelta.x, titleHeight + recipeDefinition.Inputs.Length * lineHeight);

            // Outputs
            this.outputs.Definitions = recipeDefinition.Outputs.Cast<object>();
            var outputTransform = this.outputs.GetComponent<RectTransform>();
            outputTransform.anchoredPosition = new Vector2(outputTransform.anchoredPosition.x, inputTransform.anchoredPosition.y - inputTransform.rect.height - margin);
            outputTransform.sizeDelta = new Vector2(outputTransform.sizeDelta.x, titleHeight + recipeDefinition.Outputs.Length * lineHeight);

            // Duration
            this.duration.text = recipeDefinition.Duration.ToString();
            var durationTransform = this.duration.transform.parent.GetComponent<RectTransform>();
            durationTransform.anchoredPosition = new Vector2(durationTransform.anchoredPosition.x, outputTransform.anchoredPosition.y - outputTransform.rect.height - margin);

            // Description.
            var descriptionTransform = this.recipeDescription.GetComponent<RectTransform>();
            this.recipeDescription.text = recipeDefinition.Description;
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
