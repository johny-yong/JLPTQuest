using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Stage cleared, with the words the enemies turned back into. Stars and results join in step 5
    public class VictoryView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private RubyText seals;
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

        public void Show(string heading, IList<string> wordSeals, Action onAgain)
        {
            this.onAgain = onAgain;
            title.text = heading;
            seals.Markup = wordSeals.Count == 0 ? string.Empty : "Word Seals:  " + string.Join("  ", wordSeals);
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
