using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //The enemy dissolves back into the word it was garbling. Waves and results come in Phase 3
    public class VictoryView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private RubyText wordSeal;
        [SerializeField] private Button againButton;

        private Action onAgain;

        void Awake()
        {
            againButton.onClick.AddListener(() =>
            {
                Hide();
                onAgain?.Invoke();
            });
        }

        public void Show(EnemyData enemy, Action onAgain)
        {
            this.onAgain = onAgain;
            title.text = enemy.displayName + " is beaten!";
            wordSeal.Markup = string.IsNullOrEmpty(enemy.wordSeal) ? string.Empty : "Word Seal: " + enemy.wordSeal;
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
