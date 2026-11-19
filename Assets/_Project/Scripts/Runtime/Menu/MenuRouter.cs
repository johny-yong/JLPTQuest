using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    /*
        Runs the menu scene: one screen open at a time, the tab bar to hop between them,
        and the floor preview sheet on top when a floor gets picked.
        Settings remembers where it was opened from so Back goes to the right place.
        The scenery sits behind every screen: Home shows it in full, the others pull a night veil over it.
    */
    public class MenuRouter : MonoBehaviour
    {
        [SerializeField] private ContentDatabase database;
        [SerializeField] private int realm = 1; //Only Realm 1 exists so far

        [SerializeField] private MenuPanel home;
        [SerializeField] private MenuPanel tower;
        [SerializeField] private MenuPanel codex;
        [SerializeField] private MenuPanel companions;
        [SerializeField] private MenuPanel settings;
        [SerializeField] private MenuPanel title;
        [SerializeField] private MenuPanel intro;
        [SerializeField] private FloorPreviewSheet floorPreview;
        [SerializeField] private ScrollSheet scroll;
        [SerializeField] private TabBar tabBar;
        [SerializeField] private SceneryView scenery;
        [SerializeField] private CompanionRoster roster;

        private MenuScreen current = MenuScreen.Home;
        private MenuScreen beforeSettings = MenuScreen.Home;

        public ContentDatabase Database => database;
        public int Realm => realm;
        public Progress Progress => SaveSystem.Progress;
        public CompanionData Companion => roster != null ? roster.Equipped : null;

        void Awake()
        {
            foreach (MenuPanel panel in Panels())
            {
                panel.Init(this);
                panel.Close();
            }
            tabBar.Picked += Show;
        }

        void Start()
        {
            if (database == null) Debug.LogError("[Menu] There's no ContentDatabase on the MenuRouter.");
            Show(GameFlow.ReturnScreen);

            //Read the scroll after a defeat lands here with that floor's scroll open
            if (GameFlow.TakeScroll(out string stageId, out string grammarId) && database != null)
            {
                StageData stage = database.GetStage(stageId);
                if (stage != null) scroll.Show(stage, true, grammarId);
            }
        }

        void OnDestroy()
        {
            tabBar.Picked -= Show;
        }

        public void Show(MenuScreen screen)
        {
            if (screen == MenuScreen.Settings && current != MenuScreen.Settings) beforeSettings = current;

            floorPreview.Close();
            scroll.Close();
            foreach (MenuPanel panel in Panels())
                if (panel != floorPreview && panel != scroll && panel != Get(screen)) panel.Close();

            current = screen;
            if (scenery != null) scenery.SetVeil(Veil(screen));
            Get(screen).Open();
            tabBar.Select(screen);
            tabBar.SetVisible(screen != MenuScreen.Title && screen != MenuScreen.Intro);
        }

        //How far back the scene sits behind each screen
        private static float Veil(MenuScreen screen)
        {
            switch (screen)
            {
                case MenuScreen.Home: return 0f;
                case MenuScreen.Title: return 0.25f;
                case MenuScreen.Intro: return 0.55f;
                default: return 0.9f;
            }
        }

        //Settings has a back button too, it goes back to where it was opened from
        public void Back()
        {
            Show(beforeSettings);
        }

        //The first time a floor is played its scroll comes up first, after that Start goes straight in
        public void StartFloor(StageData stage, Difficulty difficulty = Difficulty.Normal)
        {
            if (stage == null) return;
            //The scroll only comes first on Normal, Hard needs a Normal clear so it's been read by then anyway
            if (difficulty == Difficulty.Normal && !Progress.HasReadScroll(stage.id)) scroll.Show(stage, true);
            else GameFlow.PlayStage(stage.id, difficulty);
        }

        public void OpenScroll(StageData stage)
        {
            if (stage != null) scroll.Show(stage, false);
        }

        //Reread one grammar point from the Codex: its lesson's floor scroll, opened on that card
        public void OpenScrollAt(GrammarData point)
        {
            if (point == null) return;
            foreach (StageData floor in Floors())
            {
                if (floor.lessons.Count != 1 || floor.lessons[0] != point.lesson) continue;
                scroll.Show(floor, false, point.id);
                return;
            }
        }

        public void PreviewFloor(StageData stage)
        {
            if (stage != null) floorPreview.Show(stage);
        }

        //This realm's floors, floor 1 first
        public List<StageData> Floors()
        {
            var floors = new List<StageData>();
            if (database == null) return floors;
            foreach (StageData stage in database.stages)
                if (stage.realm == realm) floors.Add(stage);
            floors.Sort((a, b) => a.floor.CompareTo(b.floor));
            return floors;
        }

        //A boss floor is one whose last wave is a boss
        public bool IsBoss(StageData stage)
        {
            if (stage == null || stage.waves.Count == 0 || database == null) return false;
            EnemyData last = database.GetEnemy(stage.waves[stage.waves.Count - 1]);
            return last != null && last.kind == EnemyKind.Boss;
        }

        //Every Word Seal this realm drops, in the order you'd first meet them
        public List<string> Seals()
        {
            var seals = new List<string>();
            foreach (StageData stage in Floors())
            {
                foreach (string id in stage.waves)
                {
                    EnemyData enemy = database.GetEnemy(id);
                    if (enemy != null && !string.IsNullOrEmpty(enemy.wordSeal) && !seals.Contains(enemy.wordSeal)) seals.Add(enemy.wordSeal);
                }
            }
            return seals;
        }

        private MenuPanel Get(MenuScreen screen)
        {
            switch (screen)
            {
                case MenuScreen.Tower: return tower;
                case MenuScreen.Codex: return codex;
                case MenuScreen.Companions: return companions;
                case MenuScreen.Settings: return settings;
                case MenuScreen.Title: return title;
                case MenuScreen.Intro: return intro;
                default: return home;
            }
        }

        private IEnumerable<MenuPanel> Panels()
        {
            yield return home;
            yield return tower;
            yield return codex;
            yield return companions;
            yield return settings;
            yield return title;
            yield return intro;
            yield return floorPreview;
            yield return scroll;
        }
    }
}
