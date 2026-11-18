using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class CalmModeTests
    {
        private static QuestionData Question(string id)
        {
            var question = new QuestionData { id = id, format = QuestionFormat.Strike, prompt = "わたし＿ がくせいです。", english = "I am a student." };
            question.slots.Add(new AnswerSlot
            {
                accepted = new List<string> { "は" },
                wrong = new List<string> { "を", "に", "で" }
            });
            return question;
        }

        //A big enemy so nothing dies by accident, attacks rarely
        private static BattleSession CalmFight()
        {
            var enemy = new EnemyData { id = "wall", displayName = "wall", hits = 50, attack = 20, attackCd = 9 };
            var questions = new List<QuestionData> { Question("a"), Question("b"), Question("c") };
            var session = new BattleSession(new List<EnemyData> { enemy }, questions, new BattleRules(), 5) { Calm = true };
            session.Start();
            session.NextRound();
            return session;
        }

        [Test]
        public void TheClockNeverRunsOut()
        {
            BattleSession session = CalmFight();
            session.Tick(999f);
            Assert.AreEqual(BattlePhase.Asking, session.Phase);
        }

        [Test]
        public void EveryAnswerCountsAtTheCalmSpeed()
        {
            BattleSession session = CalmFight();
            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Answer(session.Round.correctIndex);

            Assert.AreEqual(1.25f, result.damage.speed, 0.0001f);
            Assert.AreEqual(138, result.damage.total); //100 x 1.25 speed x 1.1 combo
        }

        [Test]
        public void CalmAnswersAreNeverPerfect()
        {
            BattleSession session = CalmFight();
            RoundResult result = null;
            session.RoundResolved += r => result = r;
            session.Answer(session.Round.correctIndex);
            Assert.IsFalse(result.damage.perfect);
        }

        [Test]
        public void NoExtraTimeHintWithoutAClock()
        {
            BattleSession session = CalmFight();
            Assert.IsFalse(session.CanUseHint(HintKind.ExtraTime));
            Assert.IsTrue(session.CanUseHint(HintKind.Translate));
        }

        [Test]
        public void TurningCalmOffStartsTheClockAgain()
        {
            BattleSession session = CalmFight();
            session.Tick(100f);
            session.Calm = false;
            session.Tick(999f);
            Assert.AreEqual(BattlePhase.Resolved, session.Phase); //Timed out
        }
    }
}
