using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class BattleSessionTests
    {
        private static QuestionData Strike(string id, string answer)
        {
            var question = new QuestionData { id = id, lesson = 1, format = QuestionFormat.Strike, prompt = "わたし＿ がくせいです。", rule = "test" };
            question.slots.Add(new AnswerSlot
            {
                accepted = new List<string> { answer },
                wrong = new List<string> { "を", "に", "で", "へ" }
            });
            return question;
        }

        private static BattleSession Grunt(int seed = 1)
        {
            var enemy = new EnemyData { id = "kuchi-he", displayName = "Kuchi-he", hits = 3, attack = 20, attackCd = 3 };
            var questions = new List<QuestionData> { Strike("a", "は"), Strike("b", "も"), Strike("c", "の") };
            return new BattleSession(enemy, questions, new BattleRules(), seed);
        }

        private static int WrongIndex(BattleSession session)
        {
            return (session.Round.correctIndex + 1) % session.Round.options.Count;
        }

        [Test]
        public void GruntGoesDownOnTheThirdRightAnswer()
        {
            BattleSession session = Grunt();
            var hits = new List<int>();
            session.RoundResolved += result => hits.Add(result.damage.total);
            session.Start();

            //Answering instantly: speed 1.5, combo 1.1 / 1.2 / 1.3
            session.Answer(session.Round.correctIndex);
            session.NextRound();
            session.Answer(session.Round.correctIndex);
            Assert.AreEqual(450 - 165 - 180, session.Enemy.Hp);
            Assert.AreEqual(BattlePhase.Resolved, session.Phase);

            session.NextRound();
            session.Answer(session.Round.correctIndex);

            CollectionAssert.AreEqual(new[] { 165, 180, 195 }, hits);
            Assert.IsTrue(session.Enemy.IsDefeated);
            Assert.AreEqual(BattlePhase.Victory, session.Phase);
        }

        [Test]
        public void MissResetsTheStreakAndBringsTheAttackCloser()
        {
            BattleSession session = Grunt();
            session.Start();
            session.Answer(session.Round.correctIndex);
            Assert.AreEqual(1, session.Streak);
            Assert.AreEqual(2, session.Enemy.CooldownLeft);

            RoundResult miss = null;
            session.RoundResolved += result => miss = result;
            session.NextRound();
            session.Answer(WrongIndex(session));

            Assert.IsFalse(miss.correct);
            Assert.AreEqual(0, miss.damage.total);
            Assert.AreEqual(0, session.Streak);
            Assert.IsTrue(miss.enemyAttacked); //2 - 2 hits zero
            Assert.AreEqual(3, session.Enemy.CooldownLeft, "Cooldown resets after the attack");
        }

        [Test]
        public void TwoMissesInARowTriggerAnAttack()
        {
            BattleSession session = Grunt();
            int attacks = 0;
            session.EnemyAttacked += () => attacks++;
            session.Start();

            session.Answer(WrongIndex(session));
            Assert.AreEqual(0, attacks);
            Assert.AreEqual(1, session.Enemy.CooldownLeft);

            session.NextRound();
            session.Answer(WrongIndex(session));
            Assert.AreEqual(1, attacks);
            Assert.AreEqual(3, session.Enemy.CooldownLeft);
        }

        [Test]
        public void RunningOutOfTimeCountsAsAMiss()
        {
            BattleSession session = Grunt();
            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Start();

            session.Tick(5f);
            Assert.AreEqual(BattlePhase.Asking, session.Phase);
            session.Tick(7.5f);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.timedOut);
            Assert.IsFalse(result.correct);
            Assert.AreEqual(-1, result.chosenIndex);
            Assert.AreEqual(0f, session.Round.timeLeft);
        }

        [Test]
        public void AnswersAfterTheRoundEndsAreIgnored()
        {
            BattleSession session = Grunt();
            int resolved = 0;
            session.RoundResolved += r => resolved++;
            session.Start();

            session.Answer(session.Round.correctIndex);
            int hp = session.Enemy.Hp;
            session.Answer(session.Round.correctIndex);
            session.Tick(20f);

            Assert.AreEqual(1, resolved);
            Assert.AreEqual(hp, session.Enemy.Hp);
        }

        [Test]
        public void SlowerAnswersHitSofter()
        {
            BattleSession session = Grunt();
            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Start();

            session.Tick(6f); //Half the 12 seconds gone
            session.Answer(session.Round.correctIndex);
            Assert.AreEqual(138, result.damage.total); //100 x 1.25 x 1.1 = 137.5, rounds up
        }
    }
}
