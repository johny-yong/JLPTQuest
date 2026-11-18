using UnityEngine;

namespace JLPTQuest
{
    //The menu's Settings screen, opened from 設. The controls themselves live in the shared SettingsPanel
    public class SettingsScreen : MenuPanel
    {
        [SerializeField] private SettingsPanel panel;

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            panel.BackPressed += Router.Back;
            panel.IntroPressed += () => Router.Show(MenuScreen.Intro);
            panel.ProgressReset += () => Router.Show(MenuScreen.Title); //A brand new player starts at the title, then the intro
        }

        public override void Open()
        {
            base.Open();
            panel.Refresh(true);
        }
    }
}
