using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    //All the ready questions for one lesson. Made by the importer, so don't edit these by hand
    public class QuestionSet : ScriptableObject
    {
        public int lesson;
        public List<QuestionData> questions = new List<QuestionData>();
    }
}
