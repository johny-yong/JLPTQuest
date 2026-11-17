using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class StageStarsTests
    {
        private readonly BattleRules rules = new BattleRules();

        private static StageStats Stats(int answered, int correct, int hints)
        {
            return new StageStats { answered = answered, correct = correct, hintsUsed = hints };
        }

        [Test]
        public void PerfectRunGetsThree()
        {
            Assert.AreEqual(3, StageStars.Count(true, Stats(15, 15, 0), rules));
        }

        [Test]
        public void EightyPercentIsEnoughForTheSecondStar()
        {
            CollectionAssert.AreEqual(new[] { true, true, true }, StageStars.Earned(true, Stats(15, 12, 0), rules));
            CollectionAssert.AreEqual(new[] { true, false, true }, StageStars.Earned(true, Stats(15, 11, 0), rules));
        }

        [Test]
        public void AnyHintLosesTheThirdStar()
        {
            CollectionAssert.AreEqual(new[] { true, true, false }, StageStars.Earned(true, Stats(15, 15, 1), rules));
        }

        [Test]
        public void NoClearNoStars()
        {
            Assert.AreEqual(0, StageStars.Count(false, Stats(15, 15, 0), rules));
        }
    }
}
