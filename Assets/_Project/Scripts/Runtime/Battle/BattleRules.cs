using System;

namespace JLPTQuest
{
    /*
        Every number the fight runs on, straight from the design doc.
        Plain class so the tests can make one without a ScriptableObject.
        Damage = baseDamage x format x speed x combo x hints
    */
    [Serializable]
    public class BattleRules
    {
        public int baseDamage = 100;

        public float speedBonus = 0.5f; //Answering instantly gives 1 + this, answering at the buzzer gives 1
        public float perfectSpeed = 1.3f; //Speed this high with no hints shows the Perfect stamp

        public float comboStep = 0.1f;
        public int comboCap = 10;

        public float hintMultiplier = 0.75f;

        public float strikeMultiplier = 1f;
        public float multiMultiplier = 1f;
        public float traceMultiplier = 1.5f;
        public float forgeMultiplier = 1.5f;
        public float buildMultiplier = 2f;

        public float strikeSeconds = 12f;
        public float multiSecondsPerBlank = 6f;
        public float traceSeconds = 20f;
        public float forgeSeconds = 15f;
        public float buildSeconds = 25f;
        public float parrySeconds = 10f;

        //The typical hit enemy HP is sized from, an answer at 1.25 speed on a small combo
        public float referenceSpeed = 1.25f;
        public float referenceCombo = 1.2f;

        public int cooldownTickOnCorrect = 1;
        public int cooldownTickOnWrong = 2;

        public int playerMaxHp = 100; //Survives about four failed parries
        public float parryCounterShare = 0.5f; //A blocked attack hits back for this much of a strike

        public int choiceCount = 4; //The 2x2 tiles

        public float FormatMultiplier(QuestionFormat format)
        {
            switch (format)
            {
                case QuestionFormat.Strike: return strikeMultiplier;
                case QuestionFormat.Multi: return multiMultiplier;
                case QuestionFormat.Trace: return traceMultiplier;
                case QuestionFormat.Forge: return forgeMultiplier;
                case QuestionFormat.Build: return buildMultiplier;
                default: return 0f; //Parry and sort don't deal damage
            }
        }

        public float TimeLimit(QuestionData question)
        {
            switch (question.format)
            {
                case QuestionFormat.Multi: return multiSecondsPerBlank * Math.Max(1, question.BlankCount);
                case QuestionFormat.Trace: return traceSeconds;
                case QuestionFormat.Forge: return forgeSeconds;
                case QuestionFormat.Build: return buildSeconds;
                case QuestionFormat.Parry: return parrySeconds;
                default: return strikeSeconds;
            }
        }
    }
}
