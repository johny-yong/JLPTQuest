using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class CompanionTalkTests
    {
        private static CompanionLines Lines()
        {
            return new CompanionLines
            {
                dawn = new List<string> { "Morning! Floor {floor} before school?" },
                day = new List<string> { "Lunch break? One quick floor!", "Floor {floor} next. Let's go!" },
                dusk = new List<string> { "The lanterns are lit. Floor {floor}?" },
                night = new List<string> { "Late study? I'm with you." },
                firstFloor = new List<string> { "Ready when you are. Floor 1 first!" },
                bossNext = new List<string> { "The boss is next. Stay sharp!" },
                realmCleared = new List<string>()
            };
        }

        [Test]
        public void TheTimeOfDayPicksTheLine()
        {
            Assert.AreEqual("Morning! Floor 3 before school?", CompanionTalk.Home(Lines(), HomeMoment.Floor, TimeOfDay.Dawn, 3, "Where is it?", "Ryū", 0));
            Assert.AreEqual("The lanterns are lit. Floor 3?", CompanionTalk.Home(Lines(), HomeMoment.Floor, TimeOfDay.Dusk, 3, "", "Ryū", 0));
        }

        [Test]
        public void ASpecialMomentWinsOverTheTime()
        {
            Assert.AreEqual("The boss is next. Stay sharp!", CompanionTalk.Home(Lines(), HomeMoment.BossNext, TimeOfDay.Night, 7, "", "Ryū", 0));
        }

        [Test]
        public void AnEmptyMomentFallsBackToTheTime()
        {
            Assert.AreEqual("Late study? I'm with you.", CompanionTalk.Home(Lines(), HomeMoment.RealmCleared, TimeOfDay.Night, 7, "", "Ryū", 0));
        }

        [Test]
        public void TokensAreFilledIn()
        {
            Assert.AreEqual("Floor 4: Time and days, Ryū?", CompanionTalk.Fill("Floor {floor}: {title}, {name}?", 4, "Time and days", "Ryū"));
        }

        [Test]
        public void TheSameSeedGivesTheSameLineAndAnySeedWorks()
        {
            var pool = new List<string> { "a", "", "b", "c" };
            Assert.AreEqual(CompanionTalk.Pick(pool, 5), CompanionTalk.Pick(pool, 5));
            Assert.AreEqual("b", CompanionTalk.Pick(pool, 1)); //Blank lines are skipped
            Assert.AreEqual("c", CompanionTalk.Pick(pool, -1));
            Assert.AreEqual(string.Empty, CompanionTalk.Pick(new List<string>(), 3));
            Assert.AreEqual(string.Empty, CompanionTalk.Pick(null, 3));
        }

        [Test]
        public void TheDayTurnsOverAtFour()
        {
            int lateNight = CompanionTalk.DaySeed(new DateTime(2026, 11, 20, 3, 59, 0));
            int evening = CompanionTalk.DaySeed(new DateTime(2026, 11, 19, 21, 0, 0));
            int morning = CompanionTalk.DaySeed(new DateTime(2026, 11, 20, 4, 0, 0));
            Assert.AreEqual(evening, lateNight);
            Assert.AreNotEqual(evening, morning);
        }
    }
}
