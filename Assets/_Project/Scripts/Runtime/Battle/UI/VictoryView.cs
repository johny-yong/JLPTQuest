using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Stage cleared: stars, how it went, the words the enemies turned back into, then retry or move on
    public class VictoryView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text stars;
        [SerializeField] private TMP_Text starNotes;
        [SerializeField] private TMP_Text statsLine;
        [SerializeField] private RubyText seals;
        [SerializeField] private Button againButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button towerButton;

        private static readonly string[] starNames = { "Clear", "80%+ right", "No hints" };

        private Action onAgain;
        private Action onNext;
        private Action onTower;

        void Awake()
        {
            againButton.onClick.AddListener(() =>
            {
                Hide();
                onAgain?.Invoke();
            });
            nextButton.onClick.AddListener(() =>
            {
                Hide();
                onNext?.Invoke();
            });
            towerButton.onClick.AddListener(() => onTower?.Invoke());
        }

        //onNext null hides the Next button, there's no floor after this one
        public void Show(string heading, bool[] earned, string stats, IList<string> wordSeals, Action onAgain, Action onNext, Action onTower)
        {
            this.onAgain = onAgain;
            this.onNext = onNext;
            this.onTower = onTower;
            title.text = heading;

            //Earned stars in vermilion, missed ones faint, and the note under each says what it was for
            string on = InkColors.ToTag(InkColors.Vermilion);
            string off = InkColors.ToTag(InkColors.InkFaint);
            string starText = string.Empty;
            string notes = string.Empty;
            for (int i = 0; i < earned.Length; i++)
            {
                starText += "<color=" + (earned[i] ? on : off) + ">" + (earned[i] ? "★" : "☆") + "</color>";
                notes += (i > 0 ? "   ·   " : string.Empty) + "<color=" + (earned[i] ? InkColors.ToTag(InkColors.Ink) : off) + ">" + starNames[Mathf.Min(i, starNames.Length - 1)] + "</color>";
            }
            stars.text = starText;
            starNotes.text = notes;
            statsLine.text = stats;

            seals.Markup = wordSeals.Count == 0 ? string.Empty : "Word Seals:  " + string.Join("  ", wordSeals);
            nextButton.gameObject.SetActive(onNext != null);
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
