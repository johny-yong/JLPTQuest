using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    /*
        Everything the game remembers between launches, stored as one JSON string.
        Lists instead of dictionaries because JsonUtility can't do dictionaries.
        Bump CurrentVersion when a change needs old saves converted in SaveJson.Read.
    */
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public bool introSeen; //Meet Ryū and How battles work only show on the very first launch
        public List<StageRecord> stages = new List<StageRecord>();
        public List<string> seals = new List<string>(); //Word Seals found, in the order they were found
        public List<string> scrollsRead = new List<string>(); //Floors whose scroll has been shown, so it only pops up by itself once
        public List<string> grammarSeen = new List<string>(); //Grammar points shown on a scroll card or a new-grammar card
        public SettingsData settings = new SettingsData();
    }

    [Serializable]
    public class SettingsData
    {
        public float musicVolume = 0.8f;
        public float effectsVolume = 0.8f;
        public bool calmMode;
    }

    //How one floor has gone so far on one difficulty
    [Serializable]
    public class StageRecord
    {
        public string id;
        public Difficulty difficulty; //One record per floor and difficulty, saves from before difficulties load as Normal
        public int clears;
        public bool[] stars = new bool[3]; //Each star stays once it's earned, they don't all have to come from one run
    }
}
