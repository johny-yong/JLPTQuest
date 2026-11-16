using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    //One row from the Stages tab, a single floor of the tower
    [Serializable]
    public class StageData
    {
        public string id;
        public int realm;
        public int floor;
        public string title;
        public List<int> lessons = new List<int>();
        public List<string> waves = new List<string>(); //Enemy ids in fight order
        public int sheetRow;
    }
}
