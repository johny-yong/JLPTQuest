using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Opened from 設 on any screen. Only Back works so far, the rest gets wired up in the settings step
    public class SettingsScreen : MenuPanel
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Toggle furiganaToggle;
        [SerializeField] private Button resetButton;

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            backButton.onClick.AddListener(Router.Back);

            //Shown so players know it exists, but locked on like FuriganaSettings says
            furiganaToggle.isOn = FuriganaSettings.ShowFurigana;
            furiganaToggle.interactable = false;
            resetButton.interactable = false; //Nothing to reset until progress is saved
        }
    }
}
