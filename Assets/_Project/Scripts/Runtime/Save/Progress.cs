using System.Collections.Generic;

namespace JLPTQuest
{
    /*
        What the save means for the tower: which floors are open, the stars on each, where Continue goes,
        and which Word Seals have been found. Plain C# on top of SaveData so it's all testable.
    */
    public class Progress
    {
        private readonly SaveData data;

        public Progress(SaveData data)
        {
            this.data = data;
        }

        public SaveData Data => data;
        public int SealCount => data.seals.Count;

        public StageRecord Record(string stageId)
        {
            return data.stages.Find(record => record.id == stageId);
        }

        public bool IsCleared(string stageId)
        {
            StageRecord record = Record(stageId);
            return record != null && record.clears > 0;
        }

        public bool[] Stars(string stageId)
        {
            StageRecord record = Record(stageId);
            return record == null ? new bool[3] : (bool[])record.stars.Clone();
        }

        public int StarCount(string stageId)
        {
            int count = 0;
            foreach (bool star in Stars(stageId))
                if (star) count++;
            return count;
        }

        public int TotalStars(IEnumerable<StageData> floors)
        {
            int total = 0;
            foreach (StageData floor in floors) total += StarCount(floor.id);
            return total;
        }

        //Floor 1 is always open, every other floor opens once the floor right below it is cleared
        public bool IsUnlocked(StageData stage, IEnumerable<StageData> floors)
        {
            StageData below = null;
            foreach (StageData floor in floors)
            {
                if (floor.realm != stage.realm || floor.floor >= stage.floor) continue;
                if (below == null || floor.floor > below.floor) below = floor;
            }
            return below == null || IsCleared(below.id);
        }

        //The lowest floor that isn't cleared yet, or the top one once they all are. Floors go floor 1 first
        public StageData NextFloor(IList<StageData> floors)
        {
            foreach (StageData floor in floors)
                if (!IsCleared(floor.id)) return floor;
            return floors.Count > 0 ? floors[floors.Count - 1] : null;
        }

        public bool AllCleared(IEnumerable<StageData> floors)
        {
            foreach (StageData floor in floors)
                if (!IsCleared(floor.id)) return false;
            return true;
        }

        public bool HasReadScroll(string stageId)
        {
            return data.scrollsRead.Contains(stageId);
        }

        public void MarkScrollRead(string stageId)
        {
            if (!string.IsNullOrEmpty(stageId) && !data.scrollsRead.Contains(stageId)) data.scrollsRead.Add(stageId);
        }

        public bool HasSeenGrammar(string grammarId)
        {
            return data.grammarSeen.Contains(grammarId);
        }

        //True when it's the first time, so callers know to save
        public bool MarkGrammarSeen(string grammarId)
        {
            if (string.IsNullOrEmpty(grammarId) || data.grammarSeen.Contains(grammarId)) return false;
            data.grammarSeen.Add(grammarId);
            return true;
        }

        public bool HasSeal(string seal)
        {
            return data.seals.Contains(seal);
        }

        //Adds a clear, keeps any new stars, and hands back the seals found for the first time
        public List<string> RecordClear(string stageId, bool[] stars, IEnumerable<string> seals)
        {
            StageRecord record = Record(stageId);
            if (record == null)
            {
                record = new StageRecord { id = stageId };
                data.stages.Add(record);
            }

            record.clears++;
            for (int i = 0; i < record.stars.Length && stars != null && i < stars.Length; i++)
                record.stars[i] |= stars[i];

            var found = new List<string>();
            if (seals == null) return found;
            foreach (string seal in seals)
            {
                if (string.IsNullOrEmpty(seal) || data.seals.Contains(seal)) continue;
                data.seals.Add(seal);
                found.Add(seal);
            }
            return found;
        }
    }
}
