using NUnit.Framework;

namespace JLPTQuest.Tests
{
    //The worked examples from the design doc, so a tweak to the formula shows up here first
    public class DamageCalculatorTests
    {
        private readonly BattleRules rules = new BattleRules();

        [Test]
        public void WorkedExampleIs173()
        {
            //Strike, 8 of 12 seconds left, 3rd right answer in a row
            DamageBreakdown hit = DamageCalculator.Calculate(rules, QuestionFormat.Strike, 8f, 12f, 3, 0);
            Assert.AreEqual(173, hit.total);
            Assert.IsTrue(hit.perfect);
        }

        [Test]
        public void OneHintTakesAQuarterOff()
        {
            DamageBreakdown hit = DamageCalculator.Calculate(rules, QuestionFormat.Strike, 8f, 12f, 3, 1);
            Assert.AreEqual(130, hit.total);
            Assert.IsFalse(hit.perfect);
        }

        [Test]
        public void SlowAnswerStillHits()
        {
            DamageBreakdown hit = DamageCalculator.Calculate(rules, QuestionFormat.Strike, 1f, 12f, 3, 0);
            Assert.AreEqual(135, hit.total);
            Assert.IsFalse(hit.perfect);
        }

        [Test]
        public void ComboStopsGrowingAtTheCap()
        {
            int atCap = DamageCalculator.Calculate(rules, QuestionFormat.Strike, 0f, 12f, 10, 0).total;
            int pastCap = DamageCalculator.Calculate(rules, QuestionFormat.Strike, 0f, 12f, 25, 0).total;
            Assert.AreEqual(200, atCap);
            Assert.AreEqual(atCap, pastCap);
        }

        [Test]
        public void HarderFormatsHitHarder()
        {
            Assert.AreEqual(150, DamageCalculator.Calculate(rules, QuestionFormat.Trace, 0f, 20f, 0, 0).total);
            Assert.AreEqual(200, DamageCalculator.Calculate(rules, QuestionFormat.Build, 0f, 25f, 0, 0).total);
            Assert.AreEqual(0, DamageCalculator.Calculate(rules, QuestionFormat.Parry, 5f, 10f, 1, 0).total);
        }

        [Test]
        public void EnemyHpIsCountedInReferenceHits()
        {
            Assert.AreEqual(150, DamageCalculator.ReferenceHit(rules));
            Assert.AreEqual(450, DamageCalculator.EnemyHp(rules, 3));
            Assert.AreEqual(750, DamageCalculator.EnemyHp(rules, 5));
            Assert.AreEqual(1800, DamageCalculator.EnemyHp(rules, 12));
        }
    }
}
