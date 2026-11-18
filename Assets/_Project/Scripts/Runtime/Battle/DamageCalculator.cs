using System;

namespace JLPTQuest
{
    //Every part of a hit, so the UI can show why it was big or small
    public struct DamageBreakdown
    {
        public int total;
        public float format;
        public float speed;
        public float combo;
        public float hints;
        public bool perfect;
    }

    public static class DamageCalculator
    {
        //combo is the streak including this answer, so the 3rd right answer in a row passes 3
        public static DamageBreakdown Calculate(BattleRules rules, QuestionFormat format, float timeLeft, float timeLimit, int combo, int hintsUsed)
        {
            double timeShare = timeLimit > 0f ? Clamp01(timeLeft / timeLimit) : 0.0;
            return CalculateAtSpeed(rules, format, (float)(1.0 + rules.speedBonus * timeShare), combo, hintsUsed);
        }

        //Same hit with the speed already decided, Calm mode passes its fixed speed in here
        public static DamageBreakdown CalculateAtSpeed(BattleRules rules, QuestionFormat format, float speed, int combo, int hintsUsed)
        {
            var hit = new DamageBreakdown
            {
                format = rules.FormatMultiplier(format),
                speed = speed,
                combo = (float)(1.0 + rules.comboStep * Math.Min(Math.Max(combo, 0), rules.comboCap)),
                hints = (float)Math.Pow(rules.hintMultiplier, Math.Max(hintsUsed, 0))
            };

            double raw = rules.baseDamage * (double)hit.format * hit.speed * hit.combo * hit.hints;
            hit.total = (int)Math.Round(raw, MidpointRounding.AwayFromZero);
            hit.perfect = hintsUsed == 0 && hit.speed >= rules.perfectSpeed && hit.total > 0;
            return hit;
        }

        //What a decent answer usually deals, enemy HP is counted in these
        public static int ReferenceHit(BattleRules rules)
        {
            return (int)Math.Round(rules.baseDamage * (double)rules.referenceSpeed * rules.referenceCombo, MidpointRounding.AwayFromZero);
        }

        public static int EnemyHp(BattleRules rules, int hits)
        {
            return Math.Max(1, hits) * ReferenceHit(rules);
        }

        private static double Clamp01(double value)
        {
            return value < 0.0 ? 0.0 : value > 1.0 ? 1.0 : value;
        }
    }
}
