using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    //One question on screen: the shuffled tiles, which one is right, and the clock
    public class QuestionRound
    {
        public QuestionData question;
        public List<string> options = new List<string>();
        public int correctIndex;
        public float timeLimit;
        public float timeLeft;
        public int number; //1 for the first question of the fight

        public string CorrectOption => options[correctIndex];
        public float TimeFraction => timeLimit > 0f ? Math.Max(0f, timeLeft) / timeLimit : 0f;
    }

    //How a round ended, everything the screen needs to react
    public class RoundResult
    {
        public QuestionRound round;
        public int chosenIndex; //-1 when the time ran out
        public bool correct;
        public bool timedOut;
        public DamageBreakdown damage;
        public int streak;
        public bool enemyAttacked;
        public bool enemyDefeated;
    }

    public static class ChoiceBuilder
    {
        //Main answer plus random wrong choices, shuffled. Only the main answer goes on a tile, other right answers never show
        public static QuestionRound Build(QuestionData question, Random random, int choiceCount, float timeLimit, int number)
        {
            AnswerSlot slot = question.slots[0];
            var wrong = new List<string>(slot.wrong);
            ListShuffle.Shuffle(wrong, random);

            var options = new List<string> { slot.MainAnswer };
            for (int i = 0; i < wrong.Count && options.Count < choiceCount; i++)
                options.Add(wrong[i]);
            ListShuffle.Shuffle(options, random);

            return new QuestionRound
            {
                question = question,
                options = options,
                correctIndex = options.IndexOf(slot.MainAnswer),
                timeLimit = timeLimit,
                timeLeft = timeLimit,
                number = number
            };
        }
    }
}
