using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class TimeLimitTests
    {
        private static QuestionData Question(QuestionFormat format, string prompt)
        {
            return new QuestionData { id = "q", format = format, prompt = prompt };
        }

        [Test]
        public void FuriganaAndSpacesDontCountAsReading()
        {
            //わたし＿学生です。 is 9 characters, the がくせい reading and the space don't count
            Assert.AreEqual(9, BattleRules.ReadingLength("わたし＿ {学生|がくせい}です。"));
        }

        [Test]
        public void LongerQuestionsGetMoreTime()
        {
            var rules = new BattleRules();
            var options = new List<string> { "は", "を", "に", "で" };
            float shortOne = rules.TimeLimit(Question(QuestionFormat.Strike, "わたし＿ {学生|がくせい}です。"), options);
            float longOne = rules.TimeLimit(Question(QuestionFormat.Strike, "これは どこ＿ ワインですか。…イタリアの ワインです。"), options);
            Assert.Greater(longOne, shortOne);
        }

        [Test]
        public void ChoicesCountAtTheirWeight()
        {
            var rules = new BattleRules { minSeconds = 0f, maxSeconds = 99f };
            QuestionData question = Question(QuestionFormat.Parry, "おなまえは？");
            var replies = new List<string> { "アンナです。" }; //6 characters, read at half weight

            float expected = rules.parrySeconds + rules.secondsPerCharacter * (6 + rules.choiceReadingWeight * 6);
            Assert.AreEqual(expected, rules.TimeLimit(question, replies), 0.001f);
        }

        [Test]
        public void TheCueAddsALittle()
        {
            var rules = new BattleRules { minSeconds = 0f, maxSeconds = 99f };
            QuestionData bare = Question(QuestionFormat.Parry, "おなまえは？");
            QuestionData withFacts = Question(QuestionFormat.Parry, "おなまえは？");
            withFacts.instruction = "you: Anna, 28, student"; //19 characters once spaces are dropped

            float extra = rules.TimeLimit(withFacts, null) - rules.TimeLimit(bare, null);
            Assert.AreEqual(rules.secondsPerCharacter * rules.cueReadingWeight * 19, extra, 0.001f);
        }

        [Test]
        public void SecondsPerCharacterIsTheKnob()
        {
            QuestionData question = Question(QuestionFormat.Strike, "わたし＿ {学生|がくせい}です。");
            var options = new List<string> { "は", "を", "に", "で" };
            var slow = new BattleRules { secondsPerCharacter = 0.5f, maxSeconds = 99f };
            var fast = new BattleRules { secondsPerCharacter = 0.1f, minSeconds = 0f };
            Assert.Greater(slow.TimeLimit(question, options), fast.TimeLimit(question, options));
        }

        [Test]
        public void StaysBetweenMinAndMax()
        {
            var rules = new BattleRules();
            Assert.AreEqual(rules.minSeconds, rules.TimeLimit(Question(QuestionFormat.Strike, "＿"), new List<string> { "a" }));

            string wall = new string('あ', 200);
            Assert.AreEqual(rules.maxSeconds, rules.TimeLimit(Question(QuestionFormat.Strike, wall), new List<string> { "a" }));
        }

        [Test]
        public void ParryTurnsUseTheParryBaseEvenForAStandInQuestion()
        {
            var rules = new BattleRules { minSeconds = 0f };
            QuestionData strike = Question(QuestionFormat.Strike, "＿");
            float asParry = rules.TimeLimit(strike, null, true);
            Assert.AreEqual(rules.parrySeconds + rules.secondsPerCharacter * 1f, asParry, 0.001f);
        }
    }
}
