using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class RubyMarkupTests
    {
        [Test]
        public void ParseSplitsPlainTextAndKanji()
        {
            var segments = RubyMarkup.Parse("わたしの{先生|せんせい}です");

            Assert.AreEqual(3, segments.Count);
            Assert.AreEqual("わたしの", segments[0].text);
            Assert.IsFalse(segments[0].HasReading);
            Assert.AreEqual("先生", segments[1].text);
            Assert.AreEqual("せんせい", segments[1].reading);
            Assert.AreEqual("です", segments[2].text);
        }

        [Test]
        public void StripAndReading()
        {
            Assert.AreEqual("わたしの先生", RubyMarkup.StripReadings("わたしの{先生|せんせい}"));
            Assert.AreEqual("わたしのせんせい", RubyMarkup.ToReading("わたしの{先生|せんせい}"));
        }

        [Test]
        public void BrokenMarkupStaysAsPlainText()
        {
            var segments = RubyMarkup.Parse("{先生せんせい}");
            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual("{先生せんせい}", segments[0].text);
        }

        [TestCase("わたしの{先生|せんせい}", true)]
        [TestCase("{先生|せんせい", false)]
        [TestCase("先生|せんせい}", false)]
        [TestCase("{先生|sensei}", false)]
        [TestCase("{先{生|せい}|せん}", false)]
        [TestCase("{先生}", false)]
        public void Validate(string markup, bool valid)
        {
            Assert.AreEqual(valid, RubyMarkup.TryValidate(markup, out string problem), problem);
        }

        [Test]
        public void RichTextCentresTheReading()
        {
            //One em per character keeps the maths easy to follow
            string rich = RubyMarkup.ToRichText("{先生|せんせい}", text => text.Length, 0.5f, 1f);
            Assert.AreEqual("<nobr>先生<space=-2em><voffset=1em><size=50%>せんせい</size></voffset><space=0em></nobr>", rich);
        }

        [Test]
        public void RichTextPadsKanjiWhenTheReadingIsWider()
        {
            string rich = RubyMarkup.ToRichText("{円|えんえん}", text => text.Length, 0.5f, 1f);
            Assert.AreEqual("<nobr><space=0.5em>円<space=0.5em><space=-2em><voffset=1em><size=50%>えんえん</size></voffset><space=0em></nobr>", rich);
        }
    }
}
