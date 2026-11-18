using System;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The ‖ button in the top bar and the overlay it opens.
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

        public event Action PauseRequested;
        public event Action ResumeRequested;
        public event Action QuitRequested;

        void Awake()
        {
            pauseButton.onClick.AddListener(() => PauseRequested?.Invoke());
            resumeButton.onClick.AddListener(() => ResumeRequested?.Invoke());
            quitButton.onClick.AddListener(() => QuitRequested?.Invoke());
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
        }
    }
}
