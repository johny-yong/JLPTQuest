using System;

namespace JLPTQuest
{
    public enum FuriganaMode { On, Off, Auto }

    /*
        Furigana is locked on for now.
        Off and Auto are already wired up, so a settings toggle can drop in later by flipping allowChanging.
        Auto shows everything for now; hiding mastered kanji comes with the learning engine.
    */
    public static class FuriganaSettings
    {
        public static event Action Changed;

        private static readonly bool allowChanging = false;
        private static FuriganaMode mode = FuriganaMode.On;

        public static FuriganaMode Mode
        {
            get => mode;
            set
            {
                if (!allowChanging || mode == value) return;
                mode = value;
                Changed?.Invoke();
            }
        }

        public static bool ShowFurigana => mode != FuriganaMode.Off;
    }
}
