using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    //Every companion in the game, the starter first. Menu and battle both read the equipped one from here
    [CreateAssetMenu(menuName = "JLPT Quest/Companion Roster", fileName = "CompanionRoster")]
    public class CompanionRoster : ScriptableObject
    {
        public List<CompanionData> companions = new List<CompanionData>();

        //Only the starter can be equipped until drops arrive
        public CompanionData Equipped => companions.Count > 0 ? companions[0] : null;
    }
}
