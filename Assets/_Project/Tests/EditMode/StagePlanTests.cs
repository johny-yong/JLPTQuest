using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace JLPTQuest.Tests
{
    public class StagePlanTests
    {
        private ContentDatabase database;
        private readonly List<Object> made = new List<Object>();

        private static QuestionData Question(string id, QuestionFormat format)
        {
            var question = new QuestionData { id = id, format = format, prompt = "＿" };
            question.slots.Add(new AnswerSlot { accepted = new List<string> { "は" }, wrong = new List<string> { "を", "に", "で" } });
            return question;
        }

        private QuestionSet Lesson(int number, params QuestionData[] questions)
        {
            var set = ScriptableObject.CreateInstance<QuestionSet>();
            set.lesson = number;
            set.questions.AddRange(questions);
            made.Add(set);
            return set;
        }

        private static StageData Stage(string id, int[] lessons, string[] waves)
        {
            return new StageData { id = id, lessons = new List<int>(lessons), waves = new List<string>(waves) };
        }

        [SetUp]
        public void SetUp()
        {
            database = ScriptableObject.CreateInstance<ContentDatabase>();
            made.Add(database);

            database.lessons.Add(Lesson(1, Question("L01-001", QuestionFormat.Strike), Question("L01-002", QuestionFormat.Strike), Question("L01-026", QuestionFormat.Parry)));
            database.lessons.Add(Lesson(2, Question("L02-001", QuestionFormat.Strike), Question("L02-010", QuestionFormat.Trace)));
            database.lessons.Add(Lesson(3, Question("L03-026", QuestionFormat.Parry)));

            database.enemies.Add(new EnemyData { id = "kuchi-he", hits = 3, attackCd = 3 });
            database.enemies.Add(new EnemyData { id = "dokodemo", hits = 5, attackCd = 3 });

            database.stages.Add(Stage("floor", new[] { 1, 2 }, new[] { "kuchi-he", "kuchi-he", "dokodemo" }));
            database.stages.Add(Stage("ghost", new[] { 1 }, new[] { "kuchi-he", "nobody" }));
            database.stages.Add(Stage("no-strike", new[] { 3 }, new[] { "kuchi-he" }));
            database.stages.Add(Stage("empty", new[] { 1 }, new string[0]));
            database.RebuildLookups();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object thing in made) Object.DestroyImmediate(thing);
            made.Clear();
        }

        [Test]
        public void WavesKeepTheirOrderAndRepeats()
        {
            StagePlan plan = StagePlan.Build(database, "floor", out string problem);
            Assert.IsNull(problem);
            CollectionAssert.AreEqual(new[] { "kuchi-he", "kuchi-he", "dokodemo" }, plan.waves.ConvertAll(e => e.id));
        }

        [Test]
        public void OnlyStrikeQuestionsFromEveryLesson()
        {
            StagePlan plan = StagePlan.Build(database, "floor", out _);
            CollectionAssert.AreEquivalent(new[] { "L01-001", "L01-002", "L02-001" }, plan.questions.ConvertAll(q => q.id));
        }

        [Test]
        public void ParryQuestionsGoInTheirOwnPile()
        {
            StagePlan plan = StagePlan.Build(database, "floor", out _);
            CollectionAssert.AreEqual(new[] { "L01-026" }, plan.parryQuestions.ConvertAll(q => q.id));
        }

        [Test]
        public void NextFloorIsOneUpInTheSameRealm()
        {
            var first = new StageData { id = "R1-F1", realm = 1, floor = 1 };
            var second = new StageData { id = "R1-F2", realm = 1, floor = 2 };
            var otherRealm = new StageData { id = "R2-F2", realm = 2, floor = 2 };
            database.stages.Add(otherRealm);
            database.stages.Add(second);
            database.stages.Add(first);

            Assert.AreEqual("R1-F2", StagePlan.NextStageId(database, first));
            Assert.IsNull(StagePlan.NextStageId(database, second), "Top of the realm");
        }

        [Test]
        public void UnknownEnemyIsCalledOut()
        {
            Assert.IsNull(StagePlan.Build(database, "ghost", out string problem));
            StringAssert.Contains("nobody", problem);
        }

        [Test]
        public void StagesThatCantBePlayedSayWhy()
        {
            Assert.IsNull(StagePlan.Build(database, "no-strike", out string noStrike));
            StringAssert.Contains("no strike questions", noStrike);

            Assert.IsNull(StagePlan.Build(database, "empty", out string noWaves));
            StringAssert.Contains("no waves", noWaves);

            Assert.IsNull(StagePlan.Build(database, "R9-F9", out string missing));
            StringAssert.Contains("R9-F9", missing);

            Assert.IsNull(StagePlan.Build(null, "floor", out string noDatabase));
            Assert.IsNotNull(noDatabase);
        }
    }
}
