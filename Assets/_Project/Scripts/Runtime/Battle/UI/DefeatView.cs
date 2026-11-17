using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Out of HP. The full results screen with stars comes in step 5
    public class DefeatView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private Button retryButton;

        private Action onRetry;

        void Awake()
        {
            retryButton.onClick.AddListener(() =>
            {
                Hide();
                onRetry?.Invoke();
            });
        }

        public void Show(string heading, string detail, Action onRetry)
        {
            this.onRetry = onRetry;
            title.text = heading;
            subtitle.text = detail;
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
        }
    }
}
