using UnityEngine;

namespace JLPTQuest
{
    /*
        Loads the save the first time something asks for it, and writes it when told to.
        Kept in PlayerPrefs as one JSON string, which works the same on phones and in WebGL (where it lands in IndexedDB).
        Tools > JLPT Quest > Delete Save wipes it in the editor.
    */
    public static class SaveSystem
    {
        public const string Key = "jlptquest.save";

        private static SaveData data;
        private static Progress progress;

        public static SaveData Data
        {
            get
            {
                EnsureLoaded();
                return data;
            }
        }

        public static Progress Progress
        {
            get
            {
                EnsureLoaded();
                return progress;
            }
        }

        public static void Save()
        {
            if (data == null) return;
            PlayerPrefs.SetString(Key, SaveJson.Write(data));
            PlayerPrefs.Save();
        }

        //Back to a brand new player
        public static void Reset()
        {
            Use(new SaveData());
            Save();
        }

        private static void EnsureLoaded()
        {
            if (data == null) Use(SaveJson.Read(PlayerPrefs.GetString(Key, string.Empty)));
        }

        private static void Use(SaveData loaded)
        {
            data = loaded;
            progress = new Progress(data);
        }

        //Domain reload can be off in the editor, so read the save again on every play
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ForgetOnLaunch()
        {
            data = null;
            progress = null;
        }
    }
}
