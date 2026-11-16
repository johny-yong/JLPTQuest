using UnityEngine;

namespace JLPTQuest
{
    //The asset designers tweak. Rules drive the maths, the rest is how long things stay on screen
    [CreateAssetMenu(menuName = "JLPT Quest/Battle Config", fileName = "BattleConfig")]
    public class BattleConfig : ScriptableObject
    {
        public BattleRules rules = new BattleRules();

        public float revealSeconds = 0.6f; //How long the filled in sentence stays up after a right answer
        public float teachMinSeconds = 0.8f; //The teach card can't be tapped away before this
        public float teachAutoSeconds = 0f; //Closes by itself after this many seconds, 0 means wait for a tap
    }
}
