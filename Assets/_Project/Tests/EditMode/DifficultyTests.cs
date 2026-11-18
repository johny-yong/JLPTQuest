using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class DifficultyTests
    {
        private static readonly string[] WrongPool = { "が", "を", "に", "で", "へ", "と", "も", "の" };

        private static QuestionData Question(int wrongCount)
        {
            var question = new QuestionData { id = "q" + wrongCount, format = QuestionFormat.Strike, prompt = "わたし＿ がくせいです。", english = "I am a student." };
            var slot = new AnswerSlot { accepted = new List<string> { "は" } };
            for (int i = 0; i < wrongCount; i++) slot.wrong.Add(WrongPool[i]);
            question.slots.Add(slot);
            return question;
        }

        private static BattleSession Fight(Difficulty difficulty, int wrongCount, int seed = 3)
        {
            var enemy = new EnemyData { id = "wall", displayName = "wall", hits = 50, attack = 20, attackCd = 9 };
            var session = new BattleSession(new List<EnemyData> { enemy }, new List<QuestionData> { Question(wrongCount) }, new BattleRules(), seed);
            session.Difficulty = difficulty;
            session.Start();
            session.NextRound();
            return session;
        }

        private static List<StageData> Floors()
        {
            var floors = new List<StageData>();
            for (int i = 1; i <= 3; i++) floors.Add(new StageData { id = "R1-F" + i, realm = 1, floor = i });
            return floors;
        }

        [Test]
        public void EachDifficultyHasItsOwnChoicesTimeAndDropChance()
        {
            var rules = new BattleRules();
            Assert.AreEqual(4, rules.For(Difficulty.Normal).choices);
            Assert.AreEqual(6, rules.For(Difficulty.Hard).choices);
            Assert.AreEqual(8, rules.For(Difficulty.VeryHard).choices);
            Assert.AreEqual(0f, rules.For(Difficulty.Normal).extraSeconds);
            Assert.AreEqual(2f, rules.For(Difficulty.Hard).extraSeconds);
            Assert.AreEqual(4f, rules.For(Difficulty.VeryHard).extraSeconds);
            Assert.AreEqual(0.01f, rules.For(Difficulty.Normal).companionDropChance, 1e-6f);
            Assert.AreEqual(0.05f, rules.For(Difficulty.Hard).companionDropChance, 1e-6f);
            Assert.AreEqual(0.10f, rules.For(Difficulty.VeryHard).companionDropChance, 1e-6f);
        }

        [Test]
        public void HardAndVeryHardShowMoreTiles()
        {
            Assert.AreEqual(4, Fight(Difficulty.Normal, 8).Round.options.Count);
            Assert.AreEqual(6, Fight(Difficulty.Hard, 8).Round.options.Count);
            Assert.AreEqual(8, Fight(Difficulty.VeryHard, 8).Round.options.Count);
        }

        [Test]
        public void NeverMoreTilesThanTheBlankHasWrongAnswersFor()
        {
            //Verb forms only have three good wrong answers, so they stay at 4 even on Very hard
            Assert.AreEqual(4, Fight(Difficulty.VeryHard, 3).Round.options.Count);
            Assert.AreEqual(5, Fight(Difficulty.Hard, 4).Round.options.Count);
        }

        [Test]
        public void TheRightAnswerIsAlwaysOnATileAndNothingRepeats()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                QuestionRound round = Fight(Difficulty.VeryHard, 8, seed).Round;
                Assert.AreEqual("は", round.options[round.correctIndex]);
                CollectionAssert.AllItemsAreUnique(round.options);
            }
        }

        [Test]
        public void HarderDifficultiesAddTimeToTheClock()
        {
            var rules = new BattleRules();
            QuestionRound normal = Fight(Difficulty.Normal, 8).Round;
            QuestionRound hard = Fight(Difficulty.Hard, 8).Round;
            QuestionRound veryHard = Fight(Difficulty.VeryHard, 8).Round;
            Assert.AreEqual(rules.TimeLimit(normal.question, normal.options), normal.timeLimit, 1e-4f);
            Assert.AreEqual(rules.TimeLimit(hard.question, hard.options) + 2f, hard.timeLimit, 1e-4f);
            Assert.AreEqual(rules.TimeLimit(veryHard.question, veryHard.options) + 4f, veryHard.timeLimit, 1e-4f);
        }

        [Test]
        public void FiftyFiftyLeavesHalfTheTiles()
        {
            foreach (Difficulty difficulty in new[] { Difficulty.Normal, Difficulty.Hard, Difficulty.VeryHard })
            {
                BattleSession session = Fight(difficulty, 8);
                int tiles = session.Round.options.Count;
                Assert.IsTrue(session.UseHint(HintKind.FiftyFifty));
                Assert.AreEqual(tiles - tiles / 2, session.Round.removedOptions.Count, difficulty.ToString());
                CollectionAssert.DoesNotContain(session.Round.removedOptions, session.Round.correctIndex);
                CollectionAssert.AllItemsAreUnique(session.Round.removedOptions);
            }
        }

        [Test]
        public void HardOpensOnlyAfterANormalClear()
        {
            var progress = new Progress(new SaveData());
            List<StageData> floors = Floors();
            Assert.IsTrue(progress.IsOpen(floors[0], Difficulty.Normal, floors));
            Assert.IsFalse(progress.IsOpen(floors[0], Difficulty.Hard, floors));
            Assert.IsFalse(progress.IsOpen(floors[0], Difficulty.VeryHard, floors));

            progress.RecordClear("R1-F1", new[] { true, false, false }, null);
            Assert.IsTrue(progress.IsOpen(floors[0], Difficulty.Hard, floors));
            Assert.IsTrue(progress.IsOpen(floors[0], Difficulty.VeryHard, floors));
            Assert.IsFalse(progress.IsOpen(floors[1], Difficulty.Hard, floors), "Floor 2 is open but not cleared yet");
            Assert.IsFalse(progress.IsOpen(floors[2], Difficulty.Normal, floors), "Floor 3 is still locked");
        }

        [Test]
        public void EachDifficultyKeepsItsOwnStars()
        {
            var progress = new Progress(new SaveData());
            List<StageData> floors = Floors();
            progress.RecordClear("R1-F1", new[] { true, false, false }, null);
            progress.RecordClear("R1-F1", Difficulty.Hard, new[] { true, true, true }, null);

            Assert.AreEqual(1, progress.StarCount("R1-F1"));
            Assert.AreEqual(3, progress.StarCount("R1-F1", Difficulty.Hard));
            Assert.AreEqual(0, progress.StarCount("R1-F1", Difficulty.VeryHard));
            Assert.AreEqual(1, progress.TotalStars(floors), "The realm total only counts Normal");
            Assert.AreEqual(1, progress.Record("R1-F1").clears);
            Assert.AreEqual(1, progress.Record("R1-F1", Difficulty.Hard).clears);
        }

        [Test]
        public void HardestClearedPicksTheTopOne()
        {
            var progress = new Progress(new SaveData());
            Assert.IsNull(progress.HardestCleared("R1-F1"));
            progress.RecordClear("R1-F1", new[] { true, false, false }, null);
            Assert.AreEqual(Difficulty.Normal, progress.HardestCleared("R1-F1"));
            progress.RecordClear("R1-F1", Difficulty.VeryHard, new[] { false, false, false }, null);
            Assert.AreEqual(Difficulty.VeryHard, progress.HardestCleared("R1-F1"));
            progress.RecordClear("R1-F1", Difficulty.Hard, new[] { false, false, false }, null);
            Assert.AreEqual(Difficulty.VeryHard, progress.HardestCleared("R1-F1"));
        }

        [Test]
        public void SavesFromBeforeDifficultiesLoadAsNormal()
        {
            SaveData data = SaveJson.Read("{\"version\":1,\"stages\":[{\"id\":\"R1-F1\",\"clears\":2,\"stars\":[true,false,true]}]}");
            var progress = new Progress(data);
            Assert.IsTrue(progress.IsCleared("R1-F1"));
            Assert.AreEqual(new[] { true, false, true }, progress.Stars("R1-F1"));
            Assert.IsFalse(progress.IsCleared("R1-F1", Difficulty.Hard));
        }

        [Test]
        public void AHardClearSurvivesTheSave()
        {
            var progress = new Progress(new SaveData());
            progress.RecordClear("R1-F1", new[] { true, false, false }, null);
            progress.RecordClear("R1-F1", Difficulty.Hard, new[] { true, true, false }, null);

            var loaded = new Progress(SaveJson.Read(SaveJson.Write(progress.Data)));
            Assert.AreEqual(2, loaded.StarCount("R1-F1", Difficulty.Hard));
            Assert.AreEqual(1, loaded.StarCount("R1-F1"));
            Assert.AreEqual(Difficulty.Hard, loaded.HardestCleared("R1-F1"));
        }
    }
}
