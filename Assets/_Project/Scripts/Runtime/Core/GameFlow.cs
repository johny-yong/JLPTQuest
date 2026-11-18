using UnityEngine;
using UnityEngine.SceneManagement;

namespace JLPTQuest
{
    //The menu screens you can land on. The first four match the tab bar's order, the rest aren't tabs
    public enum MenuScreen { Home, Tower, Codex, Companions, Settings, Title, Intro }

    /*
        Carries the player between scenes.
        The menu picks a floor and loads Battle, the battle takes that floor once it starts,
        and anything going back to the menu says which screen to open on.
        Plain statics on purpose, it only has to live as long as the app is open.
    */
    public static class GameFlow
    {
        public const string MenuScene = "Menu";
        public const string BattleScene = "Battle";

        private static string stageToPlay;
        private static string scrollStage;
        private static string scrollGrammar;

        public static MenuScreen ReturnScreen { get; private set; } = MenuScreen.Title; //A fresh launch starts on the title screen

        public static void PlayStage(string stageId)
        {
            stageToPlay = stageId;
            SceneManager.LoadScene(BattleScene);
        }

        public static void OpenMenu(MenuScreen screen)
        {
            ReturnScreen = screen;
            SceneManager.LoadScene(MenuScene);
        }

        //Back to the menu with a floor's scroll open, on the card for grammarId if there is one
        public static void OpenScroll(string stageId, string grammarId)
        {
            scrollStage = stageId;
            scrollGrammar = grammarId;
            OpenMenu(MenuScreen.Tower);
        }

        public static bool TakeScroll(out string stageId, out string grammarId)
        {
            stageId = scrollStage;
            grammarId = scrollGrammar;
            scrollStage = null;
            scrollGrammar = null;
            return !string.IsNullOrEmpty(stageId);
        }

        //Null when nobody picked a floor, like opening the Battle scene straight in the editor
        public static string TakeStage()
        {
            string id = stageToPlay;
            stageToPlay = null;
            return id;
        }

        //Statics survive between play sessions when domain reload is off, so start clean every time
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLaunch()
        {
            stageToPlay = null;
            scrollStage = null;
            scrollGrammar = null;
            ReturnScreen = MenuScreen.Title;
        }
    }
}
