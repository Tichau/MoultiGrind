using UnityEngine;

namespace UI
{
    public abstract class UIList<TUI> : UnityEngine.MonoBehaviour
        where TUI : UnityEngine.MonoBehaviour
    {
        private readonly System.Collections.Generic.List<TUI> uiElements = new System.Collections.Generic.List<TUI>();

        [UnityEngine.SerializeField]
        private UnityEngine.GameObject prefab = null;

        [UnityEngine.SerializeField]
        private UnityEngine.Transform listHolder = null;
        
        protected virtual void Awake()
        {
            UnityEngine.Debug.Assert(this.prefab != null, "Prefab should be set.");
        }

        protected virtual void Start()
        {
        }

        protected void DisplayList<TGame>(System.Collections.Generic.IEnumerable<TGame> gameElements, System.Predicate<TGame> displayPredicate, System.Action<TGame, TUI> updateElement)
        {
            int index = 0;
            if (gameElements != null)
            {
                foreach (var element in gameElements)
                {
                    if (displayPredicate != null && !displayPredicate.Invoke(element))
                    {
                        continue;
                    }

                    TUI line = default(TUI);
                    if (index < uiElements.Count)
                    {
                        line = this.uiElements[index];
                    }
                    else
                    {
                        var gameObject = UnityEngine.GameObject.Instantiate(prefab);
                        gameObject.transform.SetParent(this.listHolder, false);
                        line = gameObject.GetComponent<TUI>();
                        uiElements.Add(line);
                    }

                    updateElement.Invoke(element, line);
                    index++;
                }
            }

            for (int remainingIndex = uiElements.Count - 1; remainingIndex >= index; remainingIndex--)
            {
                UnityEngine.GameObject.Destroy(uiElements[remainingIndex].gameObject);
                uiElements.RemoveAt(remainingIndex);
            }
        }
    }
}