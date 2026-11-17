using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //The sentence with its blank, the little English hint, and the timer bar along the top
    public class QuestionCardView : MonoBehaviour
    {
        [SerializeField] private RubyText prompt;
        [SerializeField] private TMP_Text instruction;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text parryTag; //"Enemy attack", only on parry turns
        [SerializeField] private BarFill timerBar;
        [SerializeField] private Image timerFill;
        [SerializeField] private float warnSeconds = 3f;

        private QuestionRound round;

        public void Show(QuestionRound round)
        {
            this.round = round;
            prompt.Markup = PromptDisplay.WithBlanks(round.question.prompt);
            background.color = round.isParry ? InkColors.ParryTint : InkColors.Card;
            parryTag.gameObject.SetActive(round.isParry);
            instruction.text = PromptDisplay.Cue(round.question.instruction);
            instruction.gameObject.SetActive(instruction.text.Length > 0);
        }

        //Between waves, nothing to answer
        public void Clear()
        {
            round = null;
            prompt.Markup = string.Empty;
            background.color = InkColors.Card;
            parryTag.gameObject.SetActive(false);
            instruction.gameObject.SetActive(false);
            timerBar.Value = 1f;
            timerFill.color = InkColors.InkSoft;
        }

        public void Reveal(string answer)
        {
            if (round == null) return;
            prompt.Markup = PromptDisplay.Filled(round.question.prompt, answer);
        }

        void Update()
        {
            if (round == null) return;
            timerBar.Value = round.TimeFraction;
            timerFill.color = round.timeLeft <= warnSeconds ? InkColors.Vermilion : InkColors.InkSoft;
        }
    }
}
