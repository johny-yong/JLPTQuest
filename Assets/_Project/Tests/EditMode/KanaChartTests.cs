using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class KanaChartTests
    {
        [Test]
        public void HiraganaHasTheUsualHundredAndFour()
        {
            //46 basic + 25 voiced + 33 combos, the same set Realm 0 plans sounds for
            Assert.AreEqual(104, KanaChart.Count(KanaScript.Hiragana));
        }

        [Test]
        public void KatakanaAddsTheLoanwordSounds()
        {
            Assert.AreEqual(116, KanaChart.Count(KanaScript.Katakana));
        }

        [Test]
        public void KatakanaOnlySectionsStayOffTheHiraganaChart()
        {
            foreach (KanaSection section in KanaChart.For(KanaScript.Hiragana))
                Assert.IsFalse(section.katakanaOnly, section.title);
        }

        [Test]
        public void EverySectionFillsWholeRows()
        {
            foreach (KanaSection section in KanaChart.Sections)
                Assert.AreEqual(0, section.entries.Length % section.columns, section.title);
        }

        [Test]
        public void EverySquareHasRomajiAndBlanksHaveNone()
        {
            foreach (KanaSection section in KanaChart.Sections)
                foreach (KanaEntry entry in section.entries)
                    Assert.AreEqual(entry.IsBlank, string.IsNullOrEmpty(entry.romaji), section.title + " " + entry.kana);
        }

        [Test]
        public void NoKanaShowsUpTwice()
        {
            foreach (KanaScript script in new[] { KanaScript.Hiragana, KanaScript.Katakana })
            {
                var seen = new HashSet<string>();
                foreach (KanaSection section in KanaChart.For(script))
                    foreach (KanaEntry entry in section.entries)
                        if (!entry.IsBlank) Assert.IsTrue(seen.Add(entry.In(script)), script + " " + entry.In(script));
            }
        }

        [Test]
        public void HiraganaShiftsOverToKatakana()
        {
            Assert.AreEqual("キャ", KanaChart.ToKatakana("きゃ"));
            Assert.AreEqual("ッ", KanaChart.ToKatakana("っ"));
            Assert.AreEqual("ヲ", KanaChart.ToKatakana("を"));
            Assert.AreEqual("ン", KanaChart.ToKatakana("ん"));
        }

        [Test]
        public void KatakanaAndOtherTextPassThrough()
        {
            Assert.AreEqual("コーヒー", KanaChart.ToKatakana("コーヒー"));
            Assert.AreEqual("SMALL ャ", KanaChart.ToKatakana("SMALL ゃ"));
            Assert.AreEqual("", KanaChart.ToKatakana(""));
        }

        [Test]
        public void SpotCheckTheRomaji()
        {
            var romaji = new Dictionary<string, string>();
            foreach (KanaSection section in KanaChart.Sections)
                foreach (KanaEntry entry in section.entries)
                    if (!entry.IsBlank) romaji[entry.kana] = entry.romaji;

            Assert.AreEqual("shi", romaji["し"]);
            Assert.AreEqual("tsu", romaji["つ"]);
            Assert.AreEqual("fu", romaji["ふ"]);
            Assert.AreEqual("wo", romaji["を"]);
            Assert.AreEqual("n", romaji["ん"]);
            Assert.AreEqual("ji", romaji["ぢ"]);
            Assert.AreEqual("cho", romaji["ちょ"]);
            Assert.AreEqual("ja", romaji["じゃ"]);
            Assert.AreEqual("fa", romaji["ファ"]);
        }
    }
}
