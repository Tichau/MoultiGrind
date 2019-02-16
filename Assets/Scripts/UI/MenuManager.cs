using UnityEngine;

namespace UI
{
    public class MenuManager : MonoBehaviour
    {
        public RectTransform MainMenu;
        public RectTransform MultiplayerMenu;

        public void Awake()
        {
            this.OpenMainMenu();
        }

        public void OpenMultiplayerMenu()
        {
            this.MainMenu.gameObject.SetActive(false);
            this.MultiplayerMenu.gameObject.SetActive(true);
        }

        public void OpenMainMenu()
        {
            this.MultiplayerMenu.gameObject.SetActive(false);
            this.MainMenu.gameObject.SetActive(true);
        }
    }
}
