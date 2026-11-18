using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class ProgressTests
    {
        private static List<StageData> Floors()
        {
            var floors = new List<StageData>();
            for (int i = 1; i <= 3; i++) floors.Add(new StageData { id = "R1-F" + i, realm = 1, floor = i });
            return floors;
        }

        [Test]
        public void OnlyTheFirstFloorStartsOpen()
        {
            var progress = new Progress(new SaveData());
            List<StageData> floors = Floors();
            Assert.IsTrue(progress.IsUnlocked(floors[0], floors));
            Assert.IsFalse(progress.IsUnlocked(floors[1], floors));
            Assert.IsFalse(progress.IsUnlocked(floors[2], floors));
        }

        [Test]
        public void ClearingAFloorOpensTheOneAbove()
        {
            var progress = new Progress(new SaveData());
            List<StageData> floors = Floors();
            progress.RecordClear("R1-F1", new[] { true, false, false }, null);
            Assert.IsTrue(progress.IsUnlocked(floors[1], floors));
            Assert.IsFalse(progress.IsUnlocked(floors[2], floors));
        }

        [Test]
        public void ContinueGoesToTheLowestUnclearedFloorThenStaysOnTop()
        {
            var progress = new Progress(new SaveData());
            List<StageData> floors = Floors();
            Assert.AreEqual("R1-F1", progress.NextFloor(floors).id);

            progress.RecordClear("R1-F1", new[] { true, true, true }, null);
            Assert.AreEqual("R1-F2", progress.NextFloor(floors).id);

            progress.RecordClear("R1-F2", new[] { true, true, true }, null);
            progress.RecordClear("R1-F3", new[] { true, true, true }, null);
            Assert.AreEqual("R1-F3", progress.NextFloor(floors).id);
            Assert.IsTrue(progress.AllCleared(floors));
        }

        [Test]
        public void StarsAddUpAcrossRuns()
        {
            var progress = new Progress(new SaveData());
            progress.RecordClear("R1-F1", new[] { true, true, false }, null);
            progress.RecordClear("R1-F1", new[] { true, false, true }, null);
            CollectionAssert.AreEqual(new[] { true, true, true }, progress.Stars("R1-F1"));
            Assert.AreEqual(2, progress.Record("R1-F1").clears);
        }

        [Test]
        public void AWorseRunNeverTakesStarsAway()
        {
            var progress = new Progress(new SaveData());
            progress.RecordClear("R1-F1", new[] { true, true, true }, null);
            progress.RecordClear("R1-F1", new[] { true, false, false }, null);
            Assert.AreEqual(3, progress.StarCount("R1-F1"));
        }

        [Test]
        public void SealsAreOnlyNewOnce()
        {
            var progress = new Progress(new SaveData());
            CollectionAssert.AreEqual(new[] { "{先生|せんせい}", "{駅|えき}" }, progress.RecordClear("R1-F1", new[] { true, false, false }, new[] { "{先生|せんせい}", "{駅|えき}" }));
            CollectionAssert.AreEqual(new[] { "{本|ほん}" }, progress.RecordClear("R1-F2", new[] { true, false, false }, new[] { "{駅|えき}", "{本|ほん}" }));
            Assert.AreEqual(3, progress.SealCount);
            Assert.IsTrue(progress.HasSeal("{本|ほん}"));
        }

        [Test]
        public void AScrollCountsAsReadOnce()
        {
            var progress = new Progress(new SaveData());
            Assert.IsFalse(progress.HasReadScroll("R1-F1"));
            progress.MarkScrollRead("R1-F1");
            progress.MarkScrollRead("R1-F1");
            Assert.IsTrue(progress.HasReadScroll("R1-F1"));
            Assert.AreEqual(1, progress.Data.scrollsRead.Count);
        }

        [Test]
        public void GrammarIsOnlyNewOnce()
        {
            var progress = new Progress(new SaveData());
            Assert.IsTrue(progress.MarkGrammarSeen("G01-wa"));
            Assert.IsFalse(progress.MarkGrammarSeen("G01-wa"));
            Assert.IsTrue(progress.HasSeenGrammar("G01-wa"));
            Assert.IsFalse(progress.HasSeenGrammar("G01-mo"));
        }

        [Test]
        public void TotalStarsCountsEveryFloor()
        {
            var progress = new Progress(new SaveData());
            progress.RecordClear("R1-F1", new[] { true, true, false }, null);
            progress.RecordClear("R1-F2", new[] { true, false, false }, null);
            Assert.AreEqual(3, progress.TotalStars(Floors()));
        }
    }
}
