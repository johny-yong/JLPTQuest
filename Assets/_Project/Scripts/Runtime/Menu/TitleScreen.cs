using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //First thing you see when the app opens: the name of the game, tap anywhere to go on (the intro the very first time, Home after)
    public class TitleScreen : MenuPanel
    {
        [SerializeField] private Button startButton; //Covers the whole screen
        [SerializeField] private CanvasGroup tapHint;
        [SerializeField] private float pulseSpeed = 3f;

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            startButton.onClick.AddListener(() => Router.Show(SaveSystem.Data.introSeen ? MenuScreen.Home : MenuScreen.Intro));
        }

        //Slow breathing on "Tap to start" so it reads as something to do
        void Update()
        {
            if (!IsOpen) return;
            tapHint.alpha = Mathf.Lerp(0.35f, 1f, (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f);
        }
    }
}
