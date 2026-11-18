using System;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The ‖ button in the top bar and the overlay it opens, with Settings one tap further in.
        Hides the question and answers while it's up, so pausing can't be used to read them for free.
        The controller does the actual stopping, this only shows things and reports button presses.
    */
    public class PauseView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private CanvasGroup[] hideWhilePaused; //The question card and the answer tiles
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsPanel settings;
        [SerializeField] private CanvasGroup settingsGroup;

        public event Action PauseRequested;
        public event Action ResumeRequested;
        public event Action QuitRequested;

        void Awake()
        {
            pauseButton.onClick.AddListener(() => PauseRequested?.Invoke());
            resumeButton.onClick.AddListener(() => ResumeRequested?.Invoke());
            quitButton.onClick.AddListener(() => QuitRequested?.Invoke());
            settingsButton.onClick.AddListener(() => ShowSettings(true));
            settings.BackPressed += () => ShowSettings(false);
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
            foreach (CanvasGroup hidden in hideWhilePaused) hidden.alpha = visible ? 0f : 1f;
            if (!visible) ShowSettings(false);
        }

        //Same settings as the menu, minus Replay intro and Reset progress
        private void ShowSettings(bool show)
        {
            if (show) settings.Refresh(false);
            settingsGroup.alpha = show ? 1f : 0f;
            settingsGroup.blocksRaycasts = show;
            settingsGroup.interactable = show;
        }
    }
}
