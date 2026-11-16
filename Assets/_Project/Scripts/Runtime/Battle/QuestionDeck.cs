using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    /*
        Deals questions like a shuffled deck: every question once, then reshuffle.
        Stand in until the learning engine picks questions in Phase 4.
    */
    public class QuestionDeck
    {
        private readonly List<QuestionData> source;
        private readonly List<QuestionData> pile = new List<QuestionData>();
        private readonly Random random;
        private QuestionData last;

        public int Count => source.Count;

        public QuestionDeck(IList<QuestionData> questions, Random random)
        {
            if (questions == null || questions.Count == 0) throw new ArgumentException("A deck needs at least one question.");
            source = new List<QuestionData>(questions);
            this.random = random;
        }

        public QuestionData Draw()
        {
            if (pile.Count == 0) Refill();
            QuestionData next = pile[pile.Count - 1];
            pile.RemoveAt(pile.Count - 1);
            last = next;
            return next;
        }

        private void Refill()
        {
            pile.AddRange(source);
            ListShuffle.Shuffle(pile, random);

            //Don't let the last question of one lap open the next lap
            int top = pile.Count - 1;
            if (top > 0 && pile[top] == last)
            {
                pile[top] = pile[0];
                pile[0] = last;
            }
        }
    }

    public static class ListShuffle
    {
        public static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T swap = list[i];
                list[i] = list[j];
                list[j] = swap;
            }
        }
    }
}
