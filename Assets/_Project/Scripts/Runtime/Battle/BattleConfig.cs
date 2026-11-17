using UnityEngine;

namespace JLPTQuest
{
    //The asset designers tweak. Rules drive the maths, the rest is how long things stay on screen
    [CreateAssetMenu(menuName = "JLPT Quest/Battle Config", fileName = "BattleConfig")]
    public class BattleConfig : ScriptableObject
    {
        public BattleRules rules = new BattleRules();

        public float revealSeconds = 0.6f; //How long the filled in sentence stays up after a right answer
        public float defeatSeconds = 0.5f; //A beaten enemy fading out
        public float waveIntroSeconds = 1.2f; //The pause while the next enemy steps in, the clock doesn't run
        public float parryIntroSeconds = 0.9f; //"Enemy attack!" before a parry, the clock doesn't run
        public float cutInSeconds = 0.7f; //The companion cut-in, the next question waits for it
        public bool cutInOnCounter = true;
        public int cutInEveryCombo = 5; //A Perfect only gets a cut-in on every 5th combo, 0 turns it off
        public float teachMinSeconds = 0.8f; //The teach card can't be tapped away before this
        public float teachAutoSeconds = 0f; //Closes by itself after this many seconds, 0 means wait for a tap
    }
}
