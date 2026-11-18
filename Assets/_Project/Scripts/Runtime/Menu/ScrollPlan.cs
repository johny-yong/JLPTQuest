using System.Collections.Generic;

namespace JLPTQuest
{
    public enum ScrollCardKind { Grammar, Words, Review }

    //One page of the scroll
    public class ScrollCard
    {
        public ScrollCardKind kind;
        public int lesson;
        public GrammarData grammar; //Grammar cards
        public List<QuestionData> examples = new List<QuestionData>(); //Grammar cards: strike questions, shown with the answer filled in
        public List<WordData> words = new List<WordData>(); //Words cards
        public List<GrammarData> review = new List<GrammarData>(); //Review cards: a whole lesson at a glance
    }

    /*
        What the scroll before a floor teaches, as plain data.
        A one-lesson floor gets a card per grammar point, each with a couple of example sentences from its own questions, then its words.
        A floor that mixes lessons (the boss) gets one review card per lesson instead, a full set of cards for six lessons would drag.
    */
    public static class ScrollPlan
    {
        public const int ExamplesPerPoint = 2;

        public static List<ScrollCard> Build(ContentDatabase database, StageData stage)
        {
            var cards = new List<ScrollCard>();
            if (database == null || stage == null || stage.lessons.Count == 0) return cards;

            if (stage.lessons.Count > 1)
            {
                foreach (int lesson in stage.lessons)
                {
                    List<GrammarData> points = database.GrammarFor(lesson);
                    if (points.Count > 0) cards.Add(new ScrollCard { kind = ScrollCardKind.Review, lesson = lesson, review = points });
                }
                return cards;
            }

            int only = stage.lessons[0];
            QuestionSet set = database.GetLesson(only);
            foreach (GrammarData point in database.GrammarFor(only))
            {
                var card = new ScrollCard { kind = ScrollCardKind.Grammar, lesson = only, grammar = point };
                if (set != null)
                {
                    foreach (QuestionData question in set.questions)
                    {
                        if (card.examples.Count >= ExamplesPerPoint) break;
                        if (question.grammar == point.id && question.format == QuestionFormat.Strike) card.examples.Add(question);
                    }
                }
                cards.Add(card);
            }

            List<WordData> words = database.WordsFor(only);
            if (words.Count > 0) cards.Add(new ScrollCard { kind = ScrollCardKind.Words, lesson = only, words = words });
            return cards;
        }

        //A worked example for the new-grammar card: another strike question on the same point, never the one about to be asked
        public static QuestionData ExampleFor(ContentDatabase database, string grammarId, string excludeId)
        {
            GrammarData point = database != null ? database.GetGrammar(grammarId) : null;
            if (point == null) return null;
            QuestionSet set = database.GetLesson(point.lesson);
            if (set == null) return null;
            foreach (QuestionData question in set.questions)
                if (question.grammar == grammarId && question.format == QuestionFormat.Strike && question.id != excludeId) return question;
            return null;
        }

        //The card that covers this grammar point, or the first card when none does
        public static int IndexOf(List<ScrollCard> cards, string grammarId)
        {
            if (string.IsNullOrEmpty(grammarId)) return 0;
            for (int i = 0; i < cards.Count; i++)
            {
                ScrollCard card = cards[i];
                if (card.grammar != null && card.grammar.id == grammarId) return i;
                if (card.review.Exists(point => point.id == grammarId)) return i;
            }
            return 0;
        }
    }
}
