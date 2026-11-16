using System;

namespace JLPTQuest
{
    //One row from the Enemies tab
    [Serializable]
    public class EnemyData
    {
        public string id;
        public string displayName;
        public string nameJp;
        public EnemyKind kind;
        public int hits; //Correct answers needed to beat it, the battle turns this into HP
        public int attack; //HP the player loses on a failed parry
        public int attackCd; //Turns between attacks
        public EnemySkill skill;
        public string faceEyes;
        public string faceNose;
        public string faceMouth;
        public string wordSeal; //The word it drops when beaten
        public int sheetRow;
    }
}
