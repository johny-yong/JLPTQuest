using System;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //First time a grammar point turns up in a fight: what it is and one worked example, then the question. The clock waits for Got it
    public class NewGrammarView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RubyText title;
        [SerializeField] private RubyText pattern;
        [SerializeField] private RubyText explanation;
        [SerializeField] private GameObject exampleBlock;
        [SerializeField] private RubyText example;
        [SerializeField] private Button gotItButton;

        private Action onDone;

        void Awake()
        {
            gotItButton.onClick.AddListener(() =>
            {
                Hide();
                onDone?.Invoke();
            });
        }

        //exampleQuestion is another question on the same point, shown already answered
        public void Show(GrammarData grammar, QuestionData exampleQuestion, Action onDone)
        {
            this.onDone = onDone;
            title.Markup = grammar.name;
            pattern.Markup = grammar.pattern;
            explanation.Markup = grammar.explanation;

            exampleBlock.SetActive(exampleQuestion != null);
            if (exampleQuestion != null)
            {
                string english = "<size=80%><color=" + InkColors.ToTag(InkColors.InkSoft) + ">" + exampleQuestion.english + "</color></size>";
                example.Markup = PromptDisplay.WithAnswer(exampleQuestion.prompt, exampleQuestion.slots[0].MainAnswer) + "\n" + english;
            }
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
