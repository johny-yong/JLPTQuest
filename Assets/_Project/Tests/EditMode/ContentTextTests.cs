using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class ContentTextTests
    {
        [Test]
        public void FullWidthSymbolsTurnIntoNormalOnes()
        {
            Assert.AreEqual("は|に;へ/{先|せん}", ContentText.NormaliseSymbols("は｜に；へ／｛先｜せん｝"));
        }

        [Test]
        public void UnderscoreRunsBecomeOneBlank()
        {
            string prompt = ContentText.NormaliseSymbols("わたし___ がくせいです。");
            Assert.AreEqual("わたし＿ がくせいです。", prompt);
            Assert.AreEqual(1, ContentText.CountBlanks(prompt));
        }

        [Test]
        public void SplitLeavesFuriganaAlone()
        {
            CollectionAssert.AreEqual(new[] { "{何|なん}", "なに" }, ContentText.SplitOutsideBraces("{何|なん}|なに", '|'));
            CollectionAssert.AreEqual(new[] { "は", "に", "へ|に" }, ContentText.SplitOutsideBraces("は ; に ; へ|に", ';'));
        }

        [Test]
        public void KanaCheck()
        {
            Assert.IsTrue(ContentText.IsKanaOnly("えき"));
            Assert.IsTrue(ContentText.IsKanaOnly("バス"));
            Assert.IsTrue(ContentText.IsKanaOnly("ジュース"));
            Assert.IsFalse(ContentText.IsKanaOnly("駅"));
            Assert.IsFalse(ContentText.IsKanaOnly("eki"));
            Assert.IsFalse(ContentText.IsKanaOnly(""));
        }

        [Test]
        public void ComparableUsesReadingsAndDropsSpaces()
        {
            Assert.AreEqual("えきへいきます", ContentText.Comparable("{駅|えき}へ {行|い}きます"));
        }
    }
}
