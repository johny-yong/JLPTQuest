using System;
using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    /*
        Everything about one companion in one asset: who they are, their art, their skill and everything they say.
        Edit the lines straight in the Inspector, no code needed. Lines can use {floor}, {title} and {name}.
        One line is picked from each list, and it stays the same for the whole day so Home doesn't flicker.
    */
    [CreateAssetMenu(menuName = "JLPT Quest/Companion", fileName = "Companion")]
    public class CompanionData : ScriptableObject
    {
        [Header("Who")]
        public string id = "ryu";
        public string displayName = "Ryū";
        public string kana = "りゅう";
        public string glyph = "龍"; //Stands in for art on small badges
        public string role = "Dragon scribe";
        [TextArea(2, 5)] public string blurb;

        [Header("Art")]
        public Sprite portrait; //Full body on a transparent background
        public bool artFacesLeft; //Ticked flips the art so they face right, toward the enemy
        public Sprite avatar; //Head and shoulders for speech rows. Empty crops the portrait
        public Vector2 homeOffset; //Nudge on Home if the art sits oddly, in canvas units
        public float homeScale = 1f;

        [Header("Skill")]
        public string skillName = "Dragon's Breath";
        public string skillGlyph = "龍";
        [TextArea(2, 4)] public string skillText = "Your next correct answer hits twice as hard.";
        public int skillCharge = 4; //Correct answers to fill it

        [Header("Lines")]
        public CompanionLines lines = new CompanionLines();
    }

    [Serializable]
    public class CompanionLines
    {
        [Header("Home, by time of day")]
        [TextArea(1, 3)] public List<string> dawn = new List<string>();
        [TextArea(1, 3)] public List<string> day = new List<string>();
        [TextArea(1, 3)] public List<string> dusk = new List<string>();
        [TextArea(1, 3)] public List<string> night = new List<string>();

        [Header("Home, special moments win over the time of day")]
        [TextArea(1, 3)] public List<string> firstFloor = new List<string>();
        [TextArea(1, 3)] public List<string> bossNext = new List<string>();
        [TextArea(1, 3)] public List<string> realmCleared = new List<string>();
        [TextArea(1, 3)] public List<string> noFloors = new List<string>();

        [Header("First launch")]
        [TextArea(2, 4)] public List<string> introMeet = new List<string>();
        [TextArea(2, 4)] public List<string> introHow = new List<string>();

        [Header("Floor scrolls")]
        [TextArea(1, 3)] public List<string> scrollFirst = new List<string>();
        [TextArea(1, 3)] public List<string> scrollNext = new List<string>();
        [TextArea(1, 3)] public List<string> scrollWords = new List<string>();
        [TextArea(1, 3)] public List<string> scrollReview = new List<string>();

        [Header("Battle")]
        [TextArea(1, 3)] public List<string> victory = new List<string>();
        [TextArea(1, 3)] public List<string> defeat = new List<string>();
        [TextArea(1, 3)] public List<string> perfect = new List<string>();
        [TextArea(1, 3)] public List<string> hurt = new List<string>();
    }
}
