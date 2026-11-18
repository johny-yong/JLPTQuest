using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Out of HP. Try the floor again or head back to the tower
    public class DefeatView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button towerButton;

        private Action onRetry;
        private Action onTower;

        void Awake()
        {
            retryButton.onClick.AddListener(() =>
            {
                Hide();
                onRetry?.Invoke();
            });
            towerButton.onClick.AddListener(() => onTower?.Invoke());
        }

        public void Show(string heading, string detail, Action onRetry, Action onTower)
        {
            this.onRetry = onRetry;
            this.onTower = onTower;
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
