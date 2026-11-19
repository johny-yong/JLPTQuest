using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    //What's going on at Home, the special ones get their own lines
    public enum HomeMoment { Floor, FirstFloor, BossNext, RealmCleared, NoFloors }

    //Picks and fills a companion's line. Plain C# so it's tested, the views only show the result
    public static class CompanionTalk
    {
        public static string Home(CompanionLines lines, HomeMoment moment, TimeOfDay time, int floor, string title, string name, int seed)
        {
            if (lines == null) return string.Empty;
            List<string> pool = ForMoment(lines, moment);
            if (IsEmpty(pool)) pool = ForTime(lines, time);
            return Fill(Pick(pool, seed), floor, title, name);
        }

        //Any other list: one line for today, or the fallback when the list is empty
        public static string Say(List<string> pool, string fallback, int seed, string name = null)
        {
            string line = Pick(pool, seed);
            return Fill(line.Length > 0 ? line : fallback, 0, null, name);
        }

        //Same seed, same line. Seeding with the day keeps Home steady until tomorrow
        public static string Pick(List<string> pool, int seed)
        {
            if (IsEmpty(pool)) return string.Empty;
            var usable = new List<string>();
            foreach (string line in pool)
                if (!string.IsNullOrWhiteSpace(line)) usable.Add(line);
            int index = ((seed % usable.Count) + usable.Count) % usable.Count;
            return usable[index];
        }

        public static string Fill(string line, int floor, string title, string name)
        {
            if (string.IsNullOrEmpty(line)) return string.Empty;
            return line.Replace("{floor}", floor.ToString())
                .Replace("{title}", title ?? string.Empty)
                .Replace("{name}", name ?? string.Empty);
        }

        //Changes once a day at 4:00, the same time the review day turns over
        public static int DaySeed(DateTime local)
        {
            return local.AddHours(-TimeOfDayClock.DawnStarts).DayOfYear;
        }

        public static List<string> ForTime(CompanionLines lines, TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Dawn: return lines.dawn;
                case TimeOfDay.Day: return lines.day;
                case TimeOfDay.Dusk: return lines.dusk;
                default: return lines.night;
            }
        }

        private static List<string> ForMoment(CompanionLines lines, HomeMoment moment)
        {
            switch (moment)
            {
                case HomeMoment.FirstFloor: return lines.firstFloor;
                case HomeMoment.BossNext: return lines.bossNext;
                case HomeMoment.RealmCleared: return lines.realmCleared;
                case HomeMoment.NoFloors: return lines.noFloors;
                default: return null;
            }
        }

        private static bool IsEmpty(List<string> pool)
        {
            if (pool == null) return true;
            foreach (string line in pool)
                if (!string.IsNullOrWhiteSpace(line)) return false;
            return true;
        }
    }
}
