using UnityEditor;

namespace JLPTQuest.EditorTools
{
    //Pins the scenery to one time of day in the editor. Auto goes back to the real clock
    public static class TimeOfDayMenu
    {
        private const string Root = "Tools/JLPT Quest/Time of day/";

        [MenuItem(Root + "Auto (clock)", false, 0)] private static void Auto() => Set(null);
        [MenuItem(Root + "Dawn", false, 20)] private static void Dawn() => Set(TimeOfDay.Dawn);
        [MenuItem(Root + "Day", false, 21)] private static void Day() => Set(TimeOfDay.Day);
        [MenuItem(Root + "Dusk", false, 22)] private static void Dusk() => Set(TimeOfDay.Dusk);
        [MenuItem(Root + "Night", false, 23)] private static void Night() => Set(TimeOfDay.Night);

        [MenuItem(Root + "Auto (clock)", true)] private static bool AutoCheck() => Check(null);
        [MenuItem(Root + "Dawn", true)] private static bool DawnCheck() => Check(TimeOfDay.Dawn);
        [MenuItem(Root + "Day", true)] private static bool DayCheck() => Check(TimeOfDay.Day);
        [MenuItem(Root + "Dusk", true)] private static bool DuskCheck() => Check(TimeOfDay.Dusk);
        [MenuItem(Root + "Night", true)] private static bool NightCheck() => Check(TimeOfDay.Night);

        private static void Set(TimeOfDay? time)
        {
            TimeOfDayClock.EditorPreview = time;
            SceneryView.RefreshAll(); //Open scenes repaint straight away, in play mode too
        }

        //Ticks the one that's active
        private static bool Check(TimeOfDay? time)
        {
            Menu.SetChecked(Root + "Auto (clock)", !TimeOfDayClock.EditorPreview.HasValue);
            foreach (TimeOfDay value in System.Enum.GetValues(typeof(TimeOfDay)))
                Menu.SetChecked(Root + value, TimeOfDayClock.EditorPreview == value);
            return true;
        }
    }
}
