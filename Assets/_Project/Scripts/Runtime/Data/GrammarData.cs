using System;

namespace JLPTQuest
{
    //One row from the Grammar tab: a grammar point the scroll teaches and questions get tagged with
    [Serializable]
    public class GrammarData
    {
        public string id;
        public int lesson;
        public string name; //"は: the topic"
        public string pattern; //"N1 は N2 です", furigana markup allowed
        public string explanation; //Two or three plain sentences, furigana markup allowed
        public int sheetRow;
    }

    //One row from the Words tab: a key word for a lesson, shown on the scroll's last card
    [Serializable]
    public class WordData
    {
        public string id;
        public int lesson;
        public string word; //{学生|がくせい}
        public string meaning;
        public int sheetRow;
    }
}
