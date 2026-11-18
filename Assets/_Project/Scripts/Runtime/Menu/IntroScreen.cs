using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //First launch only: meet Ryū, a quick how-to, then straight into Floor 1. Skipping counts as seen too
    public class IntroScreen : MenuPanel
    {
        [SerializeField] private GameObject meetPage;
        [SerializeField] private GameObject howPage;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button startButton;

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            nextButton.onClick.AddListener(() => ShowPage(false));
            backButton.onClick.AddListener(() => ShowPage(true));
            skipButton.onClick.AddListener(() =>
            {
                MarkSeen();
                Router.Show(MenuScreen.Home);
            });
            startButton.onClick.AddListener(StartFirstFloor);
        }

        public override void Open()
        {
            base.Open();
            ShowPage(true);
        }

        private void ShowPage(bool meet)
        {
            meetPage.SetActive(meet);
            howPage.SetActive(!meet);
        }

        private void StartFirstFloor()
        {
            MarkSeen();
            List<StageData> floors = Router.Floors();
            if (floors.Count > 0) Router.StartFloor(floors[0]); //Floor 1's scroll comes first
            else Router.Show(MenuScreen.Home);
        }

        private static void MarkSeen()
        {
            SaveSystem.Data.introSeen = true;
            SaveSystem.Save();
        }
    }
}
