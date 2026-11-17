using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class HintTests
    {
        private static QuestionData Question(string id, QuestionFormat format)
        {
            var question = new QuestionData { id = id, format = format, prompt = "わたし＿ がくせいです。", english = "I am a student." };
            question.slots.Add(new AnswerSlot
            {
                accepted = new List<string> { "は" },
                wrong = new List<string> { "を", "に", "で" }
            });
            return question;
        }

        //A big enemy so nothing dies by accident, attacks rarely
        private static BattleSession Fight(QuestionFormat format = QuestionFormat.Strike, BattleRules rules = null)
        {
            var enemy = new EnemyData { id = "wall", displayName = "wall", hits = 50, attack = 20, attackCd = 9 };
            var questions = new List<QuestionData> { Question("a", format), Question("b", format), Question("c", format) };
            var session = new BattleSession(new List<EnemyData> { enemy }, questions, rules ?? new BattleRules(), 5);
            session.Start();
            session.NextRound();
            return session;
        }

        private static void Right(BattleSession session)
        {
            session.Answer(session.Round.correctIndex);
            session.NextRound();
        }

        [Test]
        public void StageStartsWithTwoCharges()
        {
            Assert.AreEqual(2, Fight().HintCharges);
        }

        [Test]
        public void UsingAHintSpendsACharge()
        {
            BattleSession session = Fight();
            Assert.IsTrue(session.UseHint(HintKind.Translate));
            Assert.AreEqual(1, session.HintCharges);
            Assert.IsTrue(session.Round.UsedHint(HintKind.Translate));
            Assert.AreEqual(1, session.Stats.hintsUsed);
        }

        [Test]
        public void EachHintOncePerQuestion()
        {
            BattleSession session = Fight();
            session.UseHint(HintKind.Translate);
            Assert.IsFalse(session.UseHint(HintKind.Translate));
            Assert.AreEqual(1, session.HintCharges);
        }

        [Test]
        public void NoChargesNoHints()
        {
            BattleSession session = Fight();
            session.UseHint(HintKind.Translate);
            session.UseHint(HintKind.ExtraTime);
            Assert.AreEqual(0, session.HintCharges);
            Assert.IsFalse(session.UseHint(HintKind.FiftyFifty));
        }

        [Test]
        public void FiftyFiftyTakesTwoWrongTilesNeverTheRightOne()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var enemy = new EnemyData { id = "wall", hits = 50, attackCd = 9 };
                var session = new BattleSession(new List<EnemyData> { enemy }, new List<QuestionData> { Question("a", QuestionFormat.Strike) }, new BattleRules(), seed);
                session.Start();
                session.NextRound();
                session.UseHint(HintKind.FiftyFifty);

                List<int> removed = session.Round.removedOptions;
                Assert.AreEqual(2, removed.Count);
                Assert.AreNotEqual(removed[0], removed[1]);
                CollectionAssert.DoesNotContain(removed, session.Round.correctIndex);
            }
        }

        [Test]
        public void RemovedTilesCantBePicked()
        {
            BattleSession session = Fight();
            session.UseHint(HintKind.FiftyFifty);
            session.Answer(session.Round.removedOptions[0]);
            Assert.AreEqual(BattlePhase.Asking, session.Phase);
        }

        [Test]
        public void ExtraTimeStretchesTheLimitToo()
        {
            BattleSession session = Fight();
            float limit = session.Round.timeLimit;
            session.Tick(2f);
            session.UseHint(HintKind.ExtraTime);
            Assert.AreEqual(limit + 5f, session.Round.timeLimit, 0.001f);
            Assert.AreEqual(limit - 2f + 5f, session.Round.timeLeft, 0.001f);
        }

        [Test]
        public void DamageHintsTakeAQuarterOff()
        {
            BattleSession plain = Fight();
            RoundResult plainHit = null;
            plain.RoundResolved += r => plainHit = r;
            plain.Answer(plain.Round.correctIndex);

            BattleSession hinted = Fight();
            RoundResult hintedHit = null;
            hinted.RoundResolved += r => hintedHit = r;
            hinted.UseHint(HintKind.Translate);
            hinted.Answer(hinted.Round.correctIndex);

            Assert.AreEqual(165, plainHit.damage.total);
            Assert.AreEqual(124, hintedHit.damage.total); //165 x 0.75 = 123.75
            Assert.IsTrue(plainHit.damage.perfect);
            Assert.IsFalse(hintedHit.damage.perfect);
        }

        [Test]
        public void ExtraTimeCostsNoDamageButNoPerfectEither()
        {
            BattleSession session = Fight();
            RoundResult hit = null;
            session.RoundResolved += r => hit = r;
            session.UseHint(HintKind.ExtraTime);
            session.Answer(session.Round.correctIndex);

            Assert.AreEqual(165, hit.damage.total, "Instant answer, full speed bonus, no hint cut");
            Assert.IsFalse(hit.damage.perfect);
            Assert.AreEqual(0, session.Stats.perfects);
        }

        [Test]
        public void FirstKanaOnlyHelpsTraceAndBuild()
        {
            Assert.IsFalse(Fight(QuestionFormat.Strike).CanUseHint(HintKind.FirstKana));
            var rules = new BattleRules();
            Assert.IsTrue(rules.HintWorks(HintKind.FirstKana, QuestionFormat.Trace, false));
            Assert.IsTrue(rules.HintWorks(HintKind.FirstKana, QuestionFormat.Build, false));
            Assert.IsFalse(rules.HintWorks(HintKind.FiftyFifty, QuestionFormat.Trace, false));
            Assert.IsTrue(rules.HintWorks(HintKind.FiftyFifty, QuestionFormat.Strike, true), "Works on parry turns");
        }

        [Test]
        public void EveryFifthComboGivesAChargeBack()
        {
            BattleSession session = Fight();
            session.UseHint(HintKind.Translate);
            session.UseHint(HintKind.ExtraTime);
            Assert.AreEqual(0, session.HintCharges);

            var refills = new List<int>();
            session.RoundResolved += r => { if (r.hintRefilled) refills.Add(r.streak); };
            for (int i = 0; i < 10; i++) Right(session);

            CollectionAssert.AreEqual(new[] { 5, 10 }, refills);
            Assert.AreEqual(2, session.HintCharges);
        }

        [Test]
        public void ChargesNeverGoPastTheMax()
        {
            BattleSession session = Fight();
            for (int i = 0; i < 10; i++) Right(session);
            Assert.AreEqual(2, session.HintCharges);
        }

        [Test]
        public void HintsOnlyWhileTheQuestionIsUp()
        {
            BattleSession session = Fight();
            session.Answer(session.Round.correctIndex);
            Assert.IsFalse(session.UseHint(HintKind.Translate));
            Assert.AreEqual(2, session.HintCharges);
        }

        [Test]
        public void StatsCountRightWrongAndBestStreak()
        {
            BattleSession session = Fight();
            Right(session);
            Right(session);
            Right(session);
            session.Answer((session.Round.correctIndex + 1) % 4);
            session.NextRound();
            Right(session);

            Assert.AreEqual(5, session.Stats.answered);
            Assert.AreEqual(4, session.Stats.correct);
            Assert.AreEqual(3, session.Stats.bestStreak);
            Assert.AreEqual(0.8f, session.Stats.Accuracy, 0.001f);
        }
    }
}
