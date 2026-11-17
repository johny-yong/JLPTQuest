using System;

namespace JLPTQuest
{
    //Running totals for the results screen and its stars
    public class StageStats
    {
        public int answered;
        public int correct;
        public int hintsUsed;
        public int perfects;
        public int bestStreak;
        public int parriesBlocked;
        public int parriesFailed;

        public float Accuracy => answered > 0 ? (float)correct / answered : 0f;
    }

    /*
        Up to 3 stars, only for a cleared stage:
        one for clearing, one for getting at least starAccuracy right, one for using no hints at all.
    */
    public static class StageStars
    {
        public static bool[] Earned(bool cleared, StageStats stats, BattleRules rules)
        {
            return new[]
            {
                cleared,
                cleared && stats.answered > 0 && stats.Accuracy >= rules.starAccuracy - 0.0001f,
                cleared && stats.hintsUsed == 0
            };
        }

        public static int Count(bool cleared, StageStats stats, BattleRules rules)
        {
            int count = 0;
            foreach (bool star in Earned(cleared, stats, rules))
                if (star) count++;
            return count;
        }
    }
}
