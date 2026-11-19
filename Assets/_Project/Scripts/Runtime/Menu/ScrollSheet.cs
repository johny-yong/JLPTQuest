using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The scroll Ryū goes through before a floor: a card per grammar point with example sentences, then the floor's words.
        Comes up by itself the first time a floor is started, and any time from Read the scroll on the floor preview.
        Opened on the way into a fight, the last button and Skip both start the battle.
    */
    public class ScrollSheet : MenuPanel
    {
        [SerializeField] private TMP_Text kicker;
        [SerializeField] private TMP_Text speech;
        [SerializeField] private Image avatar;
        [SerializeField] private RubyText title;
        [SerializeField] private GameObject patternBlock;
        [SerializeField] private RubyText pattern;
        [SerializeField] private RubyText body;
        [SerializeField] private TMP_Text listLabel;
        [SerializeField] private RubyText list;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private TMP_Text pageLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text closeLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text nextLabel;

        private List<ScrollCard> cards = new List<ScrollCard>();
        private StageData stage;
        private bool startAfter;
        private int index;

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            closeButton.onClick.AddListener(Finish);
            backButton.onClick.AddListener(() => Go(index - 1));
            nextButton.onClick.AddListener(() =>
            {
                if (index >= cards.Count - 1) Finish();
                else Go(index + 1);
            });
        }

        //grammarId opens it on that point's card, for Read the scroll after a defeat
        public void Show(StageData stage, bool startAfter, string grammarId = null)
        {
            this.stage = stage;
            this.startAfter = startAfter;
            cards = ScrollPlan.Build(Router.Database, stage);
            Router.Progress.MarkScrollRead(stage.id);
            SaveSystem.Save();

            if (cards.Count == 0)
            {
                if (startAfter) GameFlow.PlayStage(stage.id); //Nothing written for this floor yet
                return;
            }

            kicker.text = (Router.IsBoss(stage) ? "BOSS FLOOR" : "FLOOR " + stage.floor) + "  ·  SCROLL";
            closeLabel.text = startAfter ? "Skip" : "Close";
            Open();
            Go(ScrollPlan.IndexOf(cards, grammarId));
        }

        private void Finish()
        {
            Close();
            if (startAfter) GameFlow.PlayStage(stage.id);
        }

        private void Go(int page)
        {
            index = Mathf.Clamp(page, 0, cards.Count - 1);
            ScrollCard card = cards[index];

            //Whatever's been on screen counts as seen, so the battle won't introduce it again
            bool marked = card.grammar != null && Router.Progress.MarkGrammarSeen(card.grammar.id);
            foreach (GrammarData point in card.review) marked |= Router.Progress.MarkGrammarSeen(point.id);
            if (marked) SaveSystem.Save();

            switch (card.kind)
            {
                case ScrollCardKind.Grammar: ShowGrammar(card); break;
                case ScrollCardKind.Words: ShowWords(card); break;
                default: ShowReview(card); break;
            }

            pageLabel.text = (index + 1) + " / " + cards.Count;
            backButton.interactable = index > 0;
            bool last = index == cards.Count - 1;
            nextLabel.text = last ? (startAfter ? "Start battle" : "Done") : "Next";

            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 1f;
        }

        private void ShowGrammar(ScrollCard card)
        {
            CompanionLines lines = Lines();
            speech.text = index == 0 ? Say(lines?.scrollFirst, "Here's what this floor is about.") : Say(lines?.scrollNext, "Next one. You'll need it up there.");
            title.Markup = card.grammar.name;
            SetPattern(card.grammar.pattern);
            SetBody(card.grammar.explanation);

            var examples = new List<string>();
            foreach (QuestionData question in card.examples)
                examples.Add(PromptDisplay.WithAnswer(question.prompt, question.slots[0].MainAnswer) + "\n" + Soft(question.english));
            SetList(examples.Count > 0 ? "EXAMPLES" : null, string.Join("\n\n", examples));
        }

        private void ShowWords(ScrollCard card)
        {
            speech.text = Say(Lines()?.scrollWords, "Words you'll run into on this floor.");
            title.Markup = "Words for Lesson " + card.lesson;
            SetPattern(null);
            SetBody(null);

            var lines = new List<string>();
            foreach (WordData word in card.words) lines.Add(word.word + "   " + Soft(word.meaning));
            SetList(null, string.Join("\n", lines));
        }

        private void ShowReview(ScrollCard card)
        {
            speech.text = Say(Lines()?.scrollReview, "A quick look back before the boss.");
            title.Markup = "Lesson " + card.lesson;
            SetPattern(null);
            SetBody(null);

            var lines = new List<string>();
            foreach (GrammarData point in card.review) lines.Add(point.name + "\n" + Soft(point.pattern));
            SetList(null, string.Join("\n\n", lines));
        }

        private void SetPattern(string markup)
        {
            patternBlock.SetActive(!string.IsNullOrEmpty(markup));
            if (patternBlock.activeSelf) pattern.Markup = markup;
        }

        private void SetBody(string markup)
        {
            body.gameObject.SetActive(!string.IsNullOrEmpty(markup));
            if (body.gameObject.activeSelf) body.Markup = markup;
        }

        private void SetList(string label, string markup)
        {
            listLabel.gameObject.SetActive(!string.IsNullOrEmpty(label));
            listLabel.text = label ?? string.Empty;
            list.Markup = markup;
        }

        //English and other second-line text, smaller and softer than the Japanese above it
        //The companion's own words for each card, from their CompanionData
        private CompanionLines Lines()
        {
            CompanionData companion = Router.Companion;
            if (avatar != null && companion != null)
            {
                Sprite face = companion.avatar != null ? companion.avatar : companion.portrait;
                if (face != null) avatar.sprite = face;
            }
            return companion != null ? companion.lines : null;
        }

        private string Say(System.Collections.Generic.List<string> pool, string fallback)
        {
            CompanionData companion = Router.Companion;
            return CompanionTalk.Say(pool, fallback, index, companion != null ? companion.displayName : null);
        }

        private static string Soft(string text)
        {
            return "<size=80%><color=" + InkColors.ToTag(InkColors.InkSoft) + ">" + text + "</color></size>";
        }
    }
}
