using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    //One blank in a question. Most formats have one, multi has two or three
    [Serializable]
    public class AnswerSlot
    {
        public List<string> accepted = new List<string>(); //First one is the main answer, the one shown on tiles
        public List<string> wrong = new List<string>();

        public string MainAnswer => accepted.Count > 0 ? accepted[0] : string.Empty;
    }

    //One row from the Questions tab, already checked by the importer
    [Serializable]
    public class QuestionData
    {
        public string id;
        public int lesson;
        public WordClass wordClass;
        public QuestionFormat format;
        public string prompt; //Keeps the {漢字|かんじ} markup and ＿ blanks
        public string instruction;
        public List<AnswerSlot> slots = new List<AnswerSlot>();
        public string rule;
        public string english;
        public string grammar; //Id from the Grammar tab, empty for rows that only practise a word
        public List<string> tags = new List<string>();
        public int sheetRow; //So a weird question can be traced back to its row in the sheet

        public int BlankCount => ContentText.CountBlanks(prompt);
    }
}
