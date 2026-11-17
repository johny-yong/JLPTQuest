using System;
using System.Collections.Generic;

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

        /*
            Timer = the format's base seconds + reading time, kept between minSeconds and maxSeconds.
            Reading time is secondsPerCharacter for every character of the question (furigana not counted),
            plus the choices at choiceReadingWeight, since those get skimmed rather than read,
            plus the English cue at cueReadingWeight, English reads fast but a parry's facts still take a moment.
            A short strike lands around 11s, a parry with four sentence replies around 16s.
        */
        public float secondsPerCharacter = 0.25f; //The one to tweak if every timer feels too short or too long
        public float choiceReadingWeight = 0.5f;
        public float cueReadingWeight = 0.15f;
        public float minSeconds = 8f;
        public float maxSeconds = 25f;

        public float strikeSeconds = 7f;
        public float multiSecondsPerBlank = 4f;
        public float traceSeconds = 14f;
        public float forgeSeconds = 9f;
        public float buildSeconds = 16f;
        public float parrySeconds = 4f;

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

        //parry is true for parry turns, even when a strike question stands in for a missing parry one
        public float TimeLimit(QuestionData question, IList<string> options, bool parry = false)
        {
            float reading = ReadingLength(question.prompt) + cueReadingWeight * ReadingLength(question.instruction);
            if (options != null)
                foreach (string option in options) reading += choiceReadingWeight * ReadingLength(option);

            float seconds = BaseSeconds(question, parry) + secondsPerCharacter * reading;
            return Math.Max(minSeconds, Math.Min(maxSeconds, seconds));
        }

        public float BaseSeconds(QuestionData question, bool parry = false)
        {
            if (parry) return parrySeconds;
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

        //What the eye actually reads: furigana and spaces don't count
        public static int ReadingLength(string markup)
        {
            int count = 0;
            foreach (char c in RubyMarkup.StripReadings(markup ?? string.Empty))
                if (!char.IsWhiteSpace(c)) count++;
            return count;
        }
    }
}
