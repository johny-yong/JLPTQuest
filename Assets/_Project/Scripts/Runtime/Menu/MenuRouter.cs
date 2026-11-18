using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    /*
        Runs the menu scene: one screen open at a time, the tab bar to hop between them,
        and the floor preview sheet on top when a floor gets picked.
        Settings remembers where it was opened from so Back goes to the right place.
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
        [SerializeField] private FloorPreviewSheet floorPreview;
        [SerializeField] private TabBar tabBar;

        private MenuScreen current = MenuScreen.Home;
        private MenuScreen beforeSettings = MenuScreen.Home;

        public ContentDatabase Database => database;
        public int Realm => realm;

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
        }

        void OnDestroy()
        {
            tabBar.Picked -= Show;
        }

        public void Show(MenuScreen screen)
        {
            if (screen == MenuScreen.Settings && current != MenuScreen.Settings) beforeSettings = current;

            floorPreview.Close();
            foreach (MenuPanel panel in Panels())
                if (panel != floorPreview && panel != Get(screen)) panel.Close();

            current = screen;
            Get(screen).Open();
            tabBar.Select(screen);
        }

        //Settings is the only screen with a back button, everything else is a tab
        public void Back()
        {
            Show(beforeSettings);
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
            yield return floorPreview;
        }
    }
}
