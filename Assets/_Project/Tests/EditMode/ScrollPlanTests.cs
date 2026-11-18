using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace JLPTQuest.Tests
{
    public class ScrollPlanTests
    {
        private ContentDatabase database;

        private static QuestionData Question(string id, int lesson, string grammar, QuestionFormat format)
        {
            var question = new QuestionData { id = id, lesson = lesson, grammar = grammar, format = format, prompt = "わたし＿ {学生|がくせい}です。", english = "I am a student." };
            question.slots.Add(new AnswerSlot { accepted = new List<string> { "は" }, wrong = new List<string> { "を", "に", "で" } });
            return question;
        }

        private static QuestionSet Lesson(int lesson, params QuestionData[] questions)
        {
            var set = ScriptableObject.CreateInstance<QuestionSet>();
            set.lesson = lesson;
            set.questions = new List<QuestionData>(questions);
            return set;
        }

        [SetUp]
        public void MakeDatabase()
        {
            database = ScriptableObject.CreateInstance<ContentDatabase>();
            database.lessons.Add(Lesson(1,
                Question("L01-001", 1, "G01-wa", QuestionFormat.Strike),
                Question("L01-002", 1, "G01-wa", QuestionFormat.Strike),
                Question("L01-003", 1, "G01-wa", QuestionFormat.Strike),
                Question("L01-004", 1, "G01-mo", QuestionFormat.Parry),
                Question("L01-005", 1, "G01-mo", QuestionFormat.Strike)));
            database.lessons.Add(Lesson(2, Question("L02-001", 2, "G02-kore", QuestionFormat.Strike)));
            database.grammar.Add(new GrammarData { id = "G01-wa", lesson = 1, name = "は" });
            database.grammar.Add(new GrammarData { id = "G01-mo", lesson = 1, name = "も" });
            database.grammar.Add(new GrammarData { id = "G02-kore", lesson = 2, name = "これ" });
            database.words.Add(new WordData { id = "W01-01", lesson = 1, word = "{学生|がくせい}", meaning = "student" });
            database.words.Add(new WordData { id = "W02-01", lesson = 2, word = "{本|ほん}", meaning = "book" });
            database.RebuildLookups();
        }

        [TearDown]
        public void Clean()
        {
            foreach (QuestionSet set in database.lessons) Object.DestroyImmediate(set);
            Object.DestroyImmediate(database);
        }

        private static StageData Floor(params int[] lessons)
        {
            return new StageData { id = "R1-F1", realm = 1, floor = 1, lessons = new List<int>(lessons) };
        }

        [Test]
        public void OneLessonFloorGetsACardPerGrammarPointThenItsWords()
        {
            List<ScrollCard> cards = ScrollPlan.Build(database, Floor(1));
            Assert.AreEqual(3, cards.Count);
            Assert.AreEqual("G01-wa", cards[0].grammar.id);
            Assert.AreEqual("G01-mo", cards[1].grammar.id);
            Assert.AreEqual(ScrollCardKind.Words, cards[2].kind);
            Assert.AreEqual(1, cards[2].words.Count, "Only this lesson's words");
        }

        [Test]
        public void ExamplesAreTwoStrikeQuestionsOfThatPoint()
        {
            List<ScrollCard> cards = ScrollPlan.Build(database, Floor(1));
            CollectionAssert.AreEqual(new[] { "L01-001", "L01-002" }, cards[0].examples.ConvertAll(q => q.id));
            CollectionAssert.AreEqual(new[] { "L01-005" }, cards[1].examples.ConvertAll(q => q.id), "Parry questions have no blank to fill");
        }

        [Test]
        public void AMixedFloorGetsAReviewCardPerLesson()
        {
            List<ScrollCard> cards = ScrollPlan.Build(database, Floor(1, 2));
            Assert.AreEqual(2, cards.Count);
            Assert.AreEqual(ScrollCardKind.Review, cards[0].kind);
            Assert.AreEqual(2, cards[0].review.Count);
            Assert.AreEqual(2, cards[1].lesson);
        }

        [Test]
        public void TheWorkedExampleIsNeverTheQuestionAboutToBeAsked()
        {
            Assert.AreEqual("L01-002", ScrollPlan.ExampleFor(database, "G01-wa", "L01-001").id);
            Assert.AreEqual("L01-001", ScrollPlan.ExampleFor(database, "G01-wa", "L01-003").id);
            Assert.IsNull(ScrollPlan.ExampleFor(database, "G01-mo", "L01-005"), "The only other one is a parry, which has no blank");
            Assert.IsNull(ScrollPlan.ExampleFor(database, "nope", "L01-001"));
        }

        [Test]
        public void IndexOfFindsTheCardForAGrammarPoint()
        {
            Assert.AreEqual(1, ScrollPlan.IndexOf(ScrollPlan.Build(database, Floor(1)), "G01-mo"));
            Assert.AreEqual(0, ScrollPlan.IndexOf(ScrollPlan.Build(database, Floor(1)), "nope"));
            Assert.AreEqual(1, ScrollPlan.IndexOf(ScrollPlan.Build(database, Floor(1, 2)), "G02-kore"));
        }
    }
}
