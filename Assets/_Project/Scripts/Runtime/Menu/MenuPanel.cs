using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //One full screen of the menu. Only the MenuRouter opens and closes these
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuPanel : MonoBehaviour
    {
        [SerializeField] private Button settingsButton; //The 設 button in the header, leave empty if the screen has none

        private CanvasGroup group;

        protected MenuRouter Router { get; private set; }
        public bool IsOpen { get; private set; }

        public virtual void Init(MenuRouter router)
        {
            Router = router;
            group = GetComponent<CanvasGroup>();
            if (settingsButton != null) settingsButton.onClick.AddListener(() => Router.Show(MenuScreen.Settings));
        }

        //Screens refresh what they show in here, so it's always up to date when it appears
        public virtual void Open()
        {
            SetVisible(true);
        }

        public void Close()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            IsOpen = visible;
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
        }
    }
}
