using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class ParryTests
    {
        private static QuestionData Question(string id, QuestionFormat format, string answer)
        {
            var question = new QuestionData { id = id, format = format, prompt = format == QuestionFormat.Parry ? "おなまえは？" : "わたし＿ がくせいです。" };
            question.slots.Add(new AnswerSlot
            {
                accepted = new List<string> { answer },
                wrong = new List<string> { "w1", "w2", "w3" }
            });
            return question;
        }

        private static EnemyData Enemy(string id, int hits, int attack = 20, int attackCd = 3)
        {
            return new EnemyData { id = id, displayName = id, hits = hits, attack = attack, attackCd = attackCd };
        }

        private static readonly List<QuestionData> strikes = new List<QuestionData>
        {
            Question("s1", QuestionFormat.Strike, "は"), Question("s2", QuestionFormat.Strike, "も"), Question("s3", QuestionFormat.Strike, "の")
        };

        private static readonly List<QuestionData> parries = new List<QuestionData>
        {
            Question("p1", QuestionFormat.Parry, "アンナです。"), Question("p2", QuestionFormat.Parry, "はい、そうです。")
        };

        //Starts the fight and gets to the first question
        private static BattleSession Fight(EnemyData enemy, List<QuestionData> parryPool = null)
        {
            var session = new BattleSession(new List<EnemyData> { enemy }, strikes, new BattleRules(), 3, parryPool ?? parries);
            session.Start();
            session.NextRound();
            return session;
        }

        private static int Wrong(BattleSession session)
        {
            return (session.Round.correctIndex + 1) % session.Round.options.Count;
        }

        //Two misses take a 3 CD enemy to its attack
        private static void MissUntilParry(BattleSession session)
        {
            session.Answer(Wrong(session));
            session.NextRound();
            session.Answer(Wrong(session));
            Assert.IsTrue(session.ParryPending);
            session.NextRound();
            Assert.IsTrue(session.Round.isParry);
        }

        [Test]
        public void CountdownAtZeroMakesTheNextTurnAParry()
        {
            BattleSession session = Fight(Enemy("grunt", 3));
            MissUntilParry(session);

            Assert.AreEqual(QuestionFormat.Parry, session.Round.question.format);
            Assert.AreEqual(new BattleRules().TimeLimit(session.Round.question, session.Round.options, true), session.Round.timeLimit);
            Assert.AreEqual(0, session.Enemy.CooldownLeft);
        }

        [Test]
        public void RightReplyBlocksAndCountersForHalfAStrike()
        {
            BattleSession session = Fight(Enemy("grunt", 3));
            MissUntilParry(session);

            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Answer(session.Round.correctIndex);

            Assert.IsTrue(result.blocked);
            Assert.AreEqual(0, result.playerDamage);
            Assert.AreEqual(100, session.Player.Hp);
            Assert.AreEqual(83, result.damage.total); //Instant strike on a 1 streak is 165, half rounds up
            Assert.AreEqual(450 - 83, session.Enemy.Hp);
            Assert.AreEqual(1, session.Streak);
            Assert.AreEqual(3, session.Enemy.CooldownLeft);
            Assert.IsFalse(session.ParryPending);
        }

        [Test]
        public void WrongReplyCostsTheEnemysAttack()
        {
            BattleSession session = Fight(Enemy("elite", 5, 25));
            MissUntilParry(session);

            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Answer(Wrong(session));

            Assert.IsFalse(result.blocked);
            Assert.AreEqual(25, result.playerDamage);
            Assert.AreEqual(75, session.Player.Hp);
            Assert.AreEqual(750, session.Enemy.Hp);
            Assert.AreEqual(0, session.Streak);
            Assert.AreEqual(3, session.Enemy.CooldownLeft);

            session.NextRound();
            Assert.IsFalse(session.Round.isParry, "Back to attacking after the parry");
        }

        [Test]
        public void ParryTimingOutIsAFailedParry()
        {
            BattleSession session = Fight(Enemy("grunt", 3));
            MissUntilParry(session);

            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Tick(session.Round.timeLimit + 0.5f);

            Assert.IsTrue(result.timedOut);
            Assert.AreEqual(20, result.playerDamage);
        }

        [Test]
        public void SurvivesFourFailedGruntParriesButNotFive()
        {
            BattleSession session = Fight(Enemy("grunt", 3));
            int defeats = 0;
            session.PlayerDefeated += () => defeats++;

            for (int parry = 1; parry <= 5; parry++)
            {
                MissUntilParry(session);
                session.Answer(Wrong(session)); //Failed parry
                if (parry < 5)
                {
                    Assert.AreEqual(100 - parry * 20, session.Player.Hp);
                    Assert.AreEqual(BattlePhase.Resolved, session.Phase);
                    session.NextRound();
                }
            }

            Assert.AreEqual(0, session.Player.Hp);
            Assert.AreEqual(BattlePhase.Defeated, session.Phase);
            Assert.AreEqual(1, defeats);

            session.NextRound(); //Nothing happens after a defeat
            session.Answer(0);
            Assert.AreEqual(BattlePhase.Defeated, session.Phase);
        }

        [Test]
        public void CounterCanFinishTheEnemy()
        {
            BattleSession session = Fight(Enemy("tiny", 1, 20, 1)); //150 HP and attacks every turn
            session.Tick(session.Round.timeLimit - 0.01f);
            session.Answer(session.Round.correctIndex); //A last second hit, 110, and the countdown hits 0
            Assert.AreEqual(40, session.Enemy.Hp);
            Assert.IsTrue(session.ParryPending);
            session.NextRound();

            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Answer(session.Round.correctIndex); //Counter on a 2 streak is half of 180

            Assert.IsTrue(result.blocked);
            Assert.AreEqual(90, result.damage.total);
            Assert.IsTrue(result.enemyDefeated);
            Assert.AreEqual(BattlePhase.StageCleared, session.Phase);
        }

        [Test]
        public void KillingTheEnemyCancelsItsAttack()
        {
            BattleSession session = Fight(Enemy("grunt", 3));
            RoundResult last = null;
            session.RoundResolved += r => last = r;
            for (int i = 0; i < 3; i++)
            {
                if (i > 0) session.NextRound();
                session.Answer(session.Round.correctIndex);
            }

            Assert.IsTrue(last.enemyDefeated);
            Assert.IsFalse(last.parryNext, "CD hit 0 on the killing blow, but a beaten enemy can't attack");
        }

        [Test]
        public void NoParryQuestionsFallsBackToOffenseOnes()
        {
            BattleSession session = Fight(Enemy("grunt", 3), new List<QuestionData>());
            MissUntilParry(session);

            Assert.AreEqual(QuestionFormat.Strike, session.Round.question.format);
            Assert.AreEqual(new BattleRules().TimeLimit(session.Round.question, session.Round.options, true), session.Round.timeLimit, "Still on the parry clock");
        }
    }
}
