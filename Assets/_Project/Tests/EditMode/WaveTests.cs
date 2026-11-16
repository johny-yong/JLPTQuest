using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class WaveTests
    {
        private static EnemyData Enemy(string id, int hits, int attackCd = 3)
        {
            return new EnemyData { id = id, displayName = id, hits = hits, attack = 20, attackCd = attackCd };
        }

        private static QuestionData Strike(string id)
        {
            var question = new QuestionData { id = id, format = QuestionFormat.Strike, prompt = "わたし＿ がくせいです。" };
            question.slots.Add(new AnswerSlot
            {
                accepted = new List<string> { "は" },
                wrong = new List<string> { "を", "に", "で" }
            });
            return question;
        }

        //Two grunts and an elite, like a normal floor
        private static BattleSession Floor()
        {
            var waves = new List<EnemyData> { Enemy("grunt-a", 3), Enemy("grunt-b", 3), Enemy("elite", 5) };
            var questions = new List<QuestionData> { Strike("q1"), Strike("q2"), Strike("q3"), Strike("q4") };
            return new BattleSession(waves, questions, new BattleRules(), 7);
        }

        //Answers right, instantly, until the current enemy falls
        private static void BeatCurrentEnemy(BattleSession session)
        {
            for (int guard = 0; guard < 50; guard++)
            {
                if (session.Phase == BattlePhase.WaveCleared || session.Phase == BattlePhase.StageCleared) return;
                if (session.Phase != BattlePhase.Asking) session.NextRound();
                session.Answer(session.Round.correctIndex);
            }
            Assert.Fail("The enemy never went down.");
        }

        [Test]
        public void WavesComeInOrderAndTheLastOneClearsTheStage()
        {
            BattleSession session = Floor();
            var started = new List<int>();
            var beaten = new List<string>();
            int cleared = 0;
            session.WaveStarted += index => started.Add(index);
            session.EnemyDefeated += enemy => beaten.Add(enemy.id);
            session.StageCleared += () => cleared++;

            session.Start();
            BeatCurrentEnemy(session);
            Assert.AreEqual(BattlePhase.WaveCleared, session.Phase);
            session.NextWave();
            BeatCurrentEnemy(session);
            Assert.AreEqual(BattlePhase.WaveCleared, session.Phase);
            session.NextWave();
            Assert.IsTrue(session.IsLastWave);
            BeatCurrentEnemy(session);

            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, started);
            CollectionAssert.AreEqual(new[] { "grunt-a", "grunt-b", "elite" }, beaten);
            Assert.AreEqual(1, cleared);
            Assert.AreEqual(BattlePhase.StageCleared, session.Phase);
        }

        [Test]
        public void NextEnemyOnlyStepsInWhenAsked()
        {
            BattleSession session = Floor();
            session.Start();
            BeatCurrentEnemy(session);

            session.NextRound(); //Not allowed between waves
            Assert.AreEqual(BattlePhase.WaveCleared, session.Phase);
            Assert.AreEqual(0, session.WaveIndex);

            session.NextWave();
            Assert.AreEqual(BattlePhase.WaveIntro, session.Phase);
            Assert.AreEqual("grunt-b", session.CurrentEnemy.id);
            Assert.AreEqual(450, session.Enemy.Hp);
            Assert.AreEqual(3, session.Enemy.CooldownLeft);
        }

        [Test]
        public void ClockWaitsDuringTheIntro()
        {
            BattleSession session = Floor();
            int resolved = 0;
            session.RoundResolved += r => resolved++;
            session.Start();

            session.Tick(100f);
            session.Answer(0);
            Assert.AreEqual(0, resolved);
            Assert.AreEqual(BattlePhase.WaveIntro, session.Phase);

            session.NextRound();
            Assert.AreEqual(BattlePhase.Asking, session.Phase);
            Assert.AreEqual(12f, session.Round.timeLeft);
        }

        [Test]
        public void StreakCarriesIntoTheNextWave()
        {
            BattleSession session = Floor();
            session.Start();
            BeatCurrentEnemy(session);
            Assert.AreEqual(3, session.Streak);

            RoundResult first = null;
            session.RoundResolved += r => first = first ?? r;
            session.NextWave();
            session.NextRound();
            session.Answer(session.Round.correctIndex);

            Assert.AreEqual(4, first.streak);
            Assert.AreEqual(210, first.damage.total); //100 x 1.5 x 1.4
        }

        [Test]
        public void StageWithNoWavesIsRefused()
        {
            Assert.Throws<System.ArgumentException>(() =>
                new BattleSession(new List<EnemyData>(), new List<QuestionData> { Strike("q") }, new BattleRules(), 1));
        }
    }
}
