using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class QuestionDealingTests
    {
        private static QuestionData Strike(string id, params string[] wrong)
        {
            var question = new QuestionData { id = id, format = QuestionFormat.Strike, prompt = "＿" };
            question.slots.Add(new AnswerSlot
            {
                accepted = new List<string> { "は", "わ" }, //The second accepted answer must never show up as a tile
                wrong = new List<string>(wrong)
            });
            return question;
        }

        [Test]
        public void TilesHaveTheMainAnswerAndThreeWrongOnes()
        {
            QuestionData question = Strike("a", "を", "に", "で", "へ", "が");
            for (int seed = 0; seed < 30; seed++)
            {
                QuestionRound round = ChoiceBuilder.Build(question, new Random(seed), 4, 12f, 1);
                Assert.AreEqual(4, round.options.Count);
                Assert.AreEqual("は", round.CorrectOption);
                Assert.AreEqual(1, round.options.FindAll(o => o == "は").Count);
                CollectionAssert.DoesNotContain(round.options, "わ");
                CollectionAssert.AllItemsAreUnique(round.options);
            }
        }

        [Test]
        public void RightAnswerMovesAroundTheTiles()
        {
            QuestionData question = Strike("a", "を", "に", "で");
            var seen = new HashSet<int>();
            for (int seed = 0; seed < 40; seed++)
                seen.Add(ChoiceBuilder.Build(question, new Random(seed), 4, 12f, 1).correctIndex);
            Assert.AreEqual(4, seen.Count);
        }

        [Test]
        public void RoundStartsWithAFullClock()
        {
            QuestionRound round = ChoiceBuilder.Build(Strike("a", "を", "に", "で"), new Random(3), 4, 12f, 7);
            Assert.AreEqual(12f, round.timeLeft);
            Assert.AreEqual(1f, round.TimeFraction);
            Assert.AreEqual(7, round.number);
        }

        [Test]
        public void DeckDealsEveryQuestionOncePerLap()
        {
            var questions = new List<QuestionData>();
            for (int i = 0; i < 6; i++) questions.Add(Strike("q" + i, "を", "に", "で"));

            var deck = new QuestionDeck(questions, new Random(5));
            for (int lap = 0; lap < 3; lap++)
            {
                var dealt = new HashSet<QuestionData>();
                for (int i = 0; i < questions.Count; i++) dealt.Add(deck.Draw());
                Assert.AreEqual(questions.Count, dealt.Count, "Lap " + lap + " repeated a question");
            }
        }

        [Test]
        public void DeckNeverRepeatsAcrossALap()
        {
            var questions = new List<QuestionData> { Strike("a", "を"), Strike("b", "を") };
            for (int seed = 0; seed < 20; seed++)
            {
                var deck = new QuestionDeck(questions, new Random(seed));
                QuestionData last = deck.Draw();
                for (int i = 0; i < 20; i++)
                {
                    QuestionData next = deck.Draw();
                    Assert.AreNotSame(last, next);
                    last = next;
                }
            }
        }

        [Test]
        public void EmptyDeckIsRefused()
        {
            Assert.Throws<ArgumentException>(() => new QuestionDeck(new List<QuestionData>(), new Random(1)));
        }
    }
}
