using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The settings controls, one prefab used in two places: the Settings screen in the menu and the pause menu in battle.
        Writes straight into GameSettings and saves when you leave. Replay intro and Reset progress only show in the menu.
    */
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectsSlider;
        [SerializeField] private Toggle calmToggle;
        [SerializeField] private Toggle furiganaToggle;
        [SerializeField] private Button introButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private TMP_Text resetLabel;
        [SerializeField] private GameObject[] menuOnly; //Hidden mid battle
        [SerializeField] private float resetConfirmSeconds = 3f;

        public event Action BackPressed;
        public event Action IntroPressed;
        public event Action ProgressReset;

        private float resetArmedUntil = -1f;

        void Awake()
        {
            backButton.onClick.AddListener(() =>
            {
                SaveSystem.Save();
                BackPressed?.Invoke();
            });
            musicSlider.onValueChanged.AddListener(value => GameSettings.MusicVolume = value);
            effectsSlider.onValueChanged.AddListener(value => GameSettings.EffectsVolume = value);
            calmToggle.onValueChanged.AddListener(on => GameSettings.CalmMode = on);
            introButton.onClick.AddListener(() => IntroPressed?.Invoke());
            resetButton.onClick.AddListener(OnReset);
            furiganaToggle.interactable = false; //Shown so players know it's coming, locked on like FuriganaSettings says
        }

        void Update()
        {
            if (resetArmedUntil > 0f && Time.unscaledTime > resetArmedUntil) Disarm();
        }

        //Fills the controls from the save without firing their change events
        public void Refresh(bool inMenu)
        {
            musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            effectsSlider.SetValueWithoutNotify(GameSettings.EffectsVolume);
            calmToggle.SetIsOnWithoutNotify(GameSettings.CalmMode);
            furiganaToggle.SetIsOnWithoutNotify(FuriganaSettings.ShowFurigana);
            foreach (GameObject item in menuOnly) item.SetActive(inMenu);
            Disarm();
        }

        //Two taps so a stray one can't wipe everything
        private void OnReset()
        {
            if (resetArmedUntil < 0f)
            {
                resetArmedUntil = Time.unscaledTime + resetConfirmSeconds;
                resetLabel.text = "Sure? Tap again";
                return;
            }

            Disarm();
            SaveSystem.Reset();
            ProgressReset?.Invoke();
        }

        private void Disarm()
        {
            resetArmedUntil = -1f;
            resetLabel.text = "Reset";
        }
    }
}
