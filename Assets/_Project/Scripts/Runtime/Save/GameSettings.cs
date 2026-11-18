using System;
using UnityEngine;

namespace JLPTQuest
{
    /*
        The player's settings, kept in the save. Changed fires on every edit so a running battle can pick up Calm mode straight away.
        The volumes are only stored for now, sound arrives in Phase 10 and reads them from here.
    */
    public static class GameSettings
    {
        public static event Action Changed;

        private static SettingsData Data => SaveSystem.Data.settings;

        public static float MusicVolume
        {
            get => Data.musicVolume;
            set
            {
                Data.musicVolume = Mathf.Clamp01(value);
                Changed?.Invoke();
            }
        }

        public static float EffectsVolume
        {
            get => Data.effectsVolume;
            set
            {
                Data.effectsVolume = Mathf.Clamp01(value);
                Changed?.Invoke();
            }
        }

        //No clock, every answer counts at a fixed speed. For learners who freeze under time pressure
        public static bool CalmMode
        {
            get => Data.calmMode;
            set
            {
                if (Data.calmMode == value) return;
                Data.calmMode = value;
                Changed?.Invoke();
            }
        }
    }
}
