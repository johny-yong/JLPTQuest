using System;

namespace JLPTQuest
{
    //Hard and Very hard open per floor once it's cleared on Normal
    public enum Difficulty { Normal, Hard, VeryHard }

    //What one difficulty changes. Only the tiles and the clock, the questions themselves stay the same
    [Serializable]
    public class DifficultyRules
    {
        public int choices = 4; //A blank never shows more than it has wrong answers for, verb forms usually stay at 4
        public float extraSeconds; //On top of the usual timer, the extra tiles take a moment to read
        public float companionDropChance = 0.01f; //Per boss clear, nothing reads it until companions drop (Phase 9)

        public DifficultyRules() { }

        public DifficultyRules(int choices, float extraSeconds, float companionDropChance)
        {
            this.choices = choices;
            this.extraSeconds = extraSeconds;
            this.companionDropChance = companionDropChance;
        }
    }

    public static class DifficultyText
    {
        public static string Name(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Hard: return "Hard";
                case Difficulty.VeryHard: return "Very hard";
                default: return "Normal";
            }
        }

        //The stamp a floor tile gets for a Hard or Very hard clear, empty for Normal
        public static string Badge(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Hard: return "難";
                case Difficulty.VeryHard: return "極";
                default: return string.Empty;
            }
        }
    }
}
