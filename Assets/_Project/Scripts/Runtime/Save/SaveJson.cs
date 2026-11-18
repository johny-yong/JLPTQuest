using System;
using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    //Turns the save into JSON and back. A broken or empty save just starts fresh instead of crashing the game
    public static class SaveJson
    {
        public static string Write(SaveData data)
        {
            return JsonUtility.ToJson(data);
        }

        public static SaveData Read(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new SaveData();

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (ArgumentException)
            {
                Debug.LogWarning("[Save] The save couldn't be read, starting a fresh one.");
                return new SaveData();
            }
            if (data == null) return new SaveData();

            Repair(data);
            return data;
        }

        //Older or hand edited saves can be missing bits, fill them in so nothing downstream sees a null
        private static void Repair(SaveData data)
        {
            if (data.stages == null) data.stages = new List<StageRecord>();
            if (data.seals == null) data.seals = new List<string>();
            if (data.scrollsRead == null) data.scrollsRead = new List<string>();
            if (data.grammarSeen == null) data.grammarSeen = new List<string>();
            if (data.settings == null) data.settings = new SettingsData();
            data.stages.RemoveAll(record => record == null || string.IsNullOrEmpty(record.id));
            foreach (StageRecord record in data.stages)
            {
                if (record.stars == null || record.stars.Length != 3)
                {
                    var stars = new bool[3];
                    if (record.stars != null) Array.Copy(record.stars, stars, Math.Min(3, record.stars.Length));
                    record.stars = stars;
                }
            }
            data.version = SaveData.CurrentVersion;
        }
    }
}
