using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        Shows up after a miss: the sentence with the right answer filled in, and the one line rule.
        Can't be tapped away straight away, so the player actually reads it.
    */
    public class TeachCardView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Button tapArea;
        [SerializeField] private TMP_Text header;
        [SerializeField] private RubyText sentence;
        [SerializeField] private TMP_Text rule;
        [SerializeField] private TMP_Text tapHint;

        private Action onClosed;
        private float shownAt;
        private float minSeconds;
        private float autoSeconds;
        private bool open;

        void Awake()
        {
            tapArea.onClick.AddListener(TryClose);
        }

        public void Show(RoundResult result, float minSeconds, float autoSeconds, Action onClosed)
        {
            this.minSeconds = minSeconds;
            this.autoSeconds = autoSeconds;
            this.onClosed = onClosed;

            QuestionData question = result.round.question;
            header.text = result.timedOut ? "Time's up" : "Not quite";
            sentence.Markup = PromptDisplay.WithAnswer(question.prompt, result.round.CorrectOption);
            rule.text = question.rule;
            tapHint.enabled = false;

            shownAt = Time.unscaledTime;
            open = true;
            SetVisible(true);
        }

        public void Hide()
        {
            open = false;
            SetVisible(false);
        }

        void Update()
        {
            if (!open) return;

            float elapsed = Time.unscaledTime - shownAt;
            tapHint.enabled = elapsed >= minSeconds;
            if (autoSeconds > 0f && elapsed >= autoSeconds) Close();
        }

        private void TryClose()
        {
            if (open && Time.unscaledTime - shownAt >= minSeconds) Close();
        }

        private void Close()
        {
            Hide();
            Action callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
        }
    }
}
