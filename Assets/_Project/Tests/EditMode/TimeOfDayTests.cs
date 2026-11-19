using System;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class TimeOfDayTests
    {
        private static TimeOfDay At(int hour, int minute = 0)
        {
            return TimeOfDayClock.At(new DateTime(2026, 11, 19, hour, minute, 0));
        }

        [Test]
        public void EachBandStartsOnItsHour()
        {
            Assert.AreEqual(TimeOfDay.Dawn, At(4));
            Assert.AreEqual(TimeOfDay.Day, At(9));
            Assert.AreEqual(TimeOfDay.Dusk, At(17));
            Assert.AreEqual(TimeOfDay.Night, At(20));
        }

        [Test]
        public void TheMinuteBeforeIsStillTheOldBand()
        {
            Assert.AreEqual(TimeOfDay.Night, At(3, 59));
            Assert.AreEqual(TimeOfDay.Dawn, At(8, 59));
            Assert.AreEqual(TimeOfDay.Day, At(16, 59));
            Assert.AreEqual(TimeOfDay.Dusk, At(19, 59));
        }

        [Test]
        public void MidnightIsNight()
        {
            Assert.AreEqual(TimeOfDay.Night, At(0));
        }

        [Test]
        public void AlwaysNightWinsOverTheClock()
        {
            var noon = new DateTime(2026, 11, 19, 12, 0, 0);
            Assert.AreEqual(TimeOfDay.Day, TimeOfDayClock.Pick(noon, false));
            Assert.AreEqual(TimeOfDay.Night, TimeOfDayClock.Pick(noon, true));
        }
    }
}
