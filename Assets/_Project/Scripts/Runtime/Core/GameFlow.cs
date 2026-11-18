using UnityEngine;
using UnityEngine.SceneManagement;

namespace JLPTQuest
{
    //The menu screens you can land on. Same order as the tab bar, Settings isn't a tab
    public enum MenuScreen { Home, Tower, Codex, Companions, Settings }

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

        public static MenuScreen ReturnScreen { get; private set; } = MenuScreen.Home;

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
            ReturnScreen = MenuScreen.Home;
        }
    }
}
