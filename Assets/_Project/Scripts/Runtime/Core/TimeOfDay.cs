using System;

namespace JLPTQuest
{
    public enum TimeOfDay { Dawn, Day, Dusk, Night }

    /*
        Which way the scenery is painted right now. Only the scene and the companion's lines change with it,
        cards, buttons and the tab bar always look the same.
        Dawn 4–9, Day 9–17, Dusk 17–20, Night 20–4, local time. Always night in Settings pins it to Night.
    */
    public static class TimeOfDayClock
    {
        public const int DawnStarts = 4;
        public const int DayStarts = 9;
        public const int DuskStarts = 17;
        public const int NightStarts = 20;

        public static TimeOfDay At(DateTime local)
        {
            int hour = local.Hour;
            if (hour >= NightStarts || hour < DawnStarts) return TimeOfDay.Night;
            if (hour >= DuskStarts) return TimeOfDay.Dusk;
            if (hour >= DayStarts) return TimeOfDay.Day;
            return TimeOfDay.Dawn;
        }

        //What the game should show now: the editor preview first, then Always night, then the clock
        public static TimeOfDay Now
        {
            get
            {
#if UNITY_EDITOR
                TimeOfDay? preview = EditorPreview;
                if (preview.HasValue) return preview.Value;
#endif
                return Pick(DateTime.Now, GameSettings.AlwaysNight);
            }
        }

        public static TimeOfDay Pick(DateTime local, bool alwaysNight)
        {
            return alwaysNight ? TimeOfDay.Night : At(local);
        }

#if UNITY_EDITOR
        public const string PreviewKey = "jq_timeOfDayPreview";

        //Set from Tools > JLPT Quest > Time of day so every scene can be checked without waiting for the sun
        public static TimeOfDay? EditorPreview
        {
            get
            {
                int value = UnityEditor.EditorPrefs.GetInt(PreviewKey, -1);
                return value < 0 ? (TimeOfDay?)null : (TimeOfDay)value;
            }
            set => UnityEditor.EditorPrefs.SetInt(PreviewKey, value.HasValue ? (int)value.Value : -1);
        }
#endif
    }
}
