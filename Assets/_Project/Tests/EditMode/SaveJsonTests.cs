using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace JLPTQuest.Tests
{
    public class SaveJsonTests
    {
        [Test]
        public void ARoundTripKeepsEverything()
        {
            var data = new SaveData();
            new Progress(data).RecordClear("R1-F1", new[] { true, false, true }, new[] { "{先生|せんせい}" });

            SaveData back = SaveJson.Read(SaveJson.Write(data));
            var progress = new Progress(back);
            CollectionAssert.AreEqual(new[] { true, false, true }, progress.Stars("R1-F1"));
            Assert.AreEqual(1, progress.Record("R1-F1").clears);
            Assert.IsTrue(progress.HasSeal("{先生|せんせい}"));
        }

        [Test]
        public void NoSaveStartsFresh()
        {
            SaveData data = SaveJson.Read(string.Empty);
            Assert.AreEqual(0, data.stages.Count);
            Assert.AreEqual(0, data.seals.Count);
        }

        [Test]
        public void ABrokenSaveStartsFreshInsteadOfCrashing()
        {
            LogAssert.Expect(LogType.Warning, "[Save] The save couldn't be read, starting a fresh one.");
            SaveData data = SaveJson.Read("{not json");
            Assert.AreEqual(0, data.stages.Count);
        }

        [Test]
        public void MissingStarsGetFilledIn()
        {
            SaveData data = SaveJson.Read("{\"version\":1,\"stages\":[{\"id\":\"R1-F1\",\"clears\":2}]}");
            Assert.AreEqual(3, data.stages[0].stars.Length);
            Assert.IsNotNull(data.seals);
        }
    }
}
