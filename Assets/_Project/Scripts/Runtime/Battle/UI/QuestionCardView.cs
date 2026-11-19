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
            background.color = round.isParry ? InkColors.ParryTint : Color.white; //The washi sprite carries its own colour
            parryTag.gameObject.SetActive(round.isParry);
            SetCue(false);
        }

        //Between waves, nothing to answer
        public void Clear()
        {
            round = null;
            prompt.Markup = string.Empty;
            background.color = Color.white;
            parryTag.gameObject.SetActive(false);
            instruction.gameObject.SetActive(false);
            timerBar.Value = 1f;
            timerFill.color = InkColors.InkSoft;
        }

        //The translate hint: the English meaning goes above the cue
        public void ShowTranslation()
        {
            if (round != null) SetCue(true);
        }

        private void SetCue(bool withTranslation)
        {
            string cue = PromptDisplay.Cue(round.question.instruction);
            string english = withTranslation ? round.question.english : string.Empty;
            if (!string.IsNullOrWhiteSpace(english))
                cue = "<i><color=" + InkColors.ToTag(InkColors.Ink) + ">" + english + "</color></i>" + (cue.Length > 0 ? "\n" + cue : string.Empty);

            instruction.text = cue;
            instruction.gameObject.SetActive(cue.Length > 0);
        }

        //Calm mode has no clock, so no timer bar either
        public void SetCalm(bool calm)
        {
            timerBar.gameObject.SetActive(!calm);
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
