namespace UI
{
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    public class ResourceList : MonoBehaviour
    {
        [SerializeField]
        private GameObject resourcePrefab;

        private List<UnityEngine.UI.Text> resourceLines = new List<UnityEngine.UI.Text>();

        private void Start()
        {
            Debug.Assert(this.resourcePrefab != null, "Resource prefab should be set.");
        }

        private void Update()
        {
            // Only display player 0 for now.
            var player = Game.Instance.Players[0];

            int index = 0;
            foreach (Resource resource in player.Resources)
            {
                UnityEngine.UI.Text resourceLine = null;
                if (index < this.resourceLines.Count)
                {
                    resourceLine = this.resourceLines[index];
                }
                else
                {
                    var gameObject = GameObject.Instantiate(this.resourcePrefab);
                    gameObject.transform.SetParent(this.transform);
                    resourceLine = gameObject.GetComponent<UnityEngine.UI.Text>();
                    this.resourceLines.Add(resourceLine);
                }

                resourceLine.text = $"{resource.Name}: {resource.Amount.ToString()} ({resource.Net})";
                index++;
            }

            for (int remainingIndex = this.resourceLines.Count - 1; remainingIndex >= index; remainingIndex--)
            {
                GameObject.Destroy(this.resourceLines[remainingIndex].gameObject);
                this.resourceLines.RemoveAt(remainingIndex);
            }
        }
    }
}
