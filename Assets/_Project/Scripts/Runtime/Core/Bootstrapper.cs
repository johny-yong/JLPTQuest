using UnityEngine;
using UnityEngine.SceneManagement;

namespace JLPTQuest
{
    //First thing that runs. Sets up app wide stuff, then hops over to the first real scene
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private string firstScene = GameFlow.MenuScene;
        [SerializeField] private int targetFrameRate = 60;

        void Start()
        {
            Application.targetFrameRate = targetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep; //Don't let the phone dim mid fight
            SceneManager.LoadScene(firstScene);
        }
    }
}
