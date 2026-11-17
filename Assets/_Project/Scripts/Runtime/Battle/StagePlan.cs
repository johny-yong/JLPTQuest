using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    /*
        Everything one stage needs, pulled out of the content database:
        the enemies in wave order and the questions it can ask from its lessons.
        Plain class so the battle rules never touch the database directly.
    */
    public class StagePlan
    {
        public StageData stage;
        public List<EnemyData> waves = new List<EnemyData>();
        public List<QuestionData> questions = new List<QuestionData>();
        public List<QuestionData> parryQuestions = new List<QuestionData>(); //What the enemies ask when they attack

        //Only strike for now, the other formats join once they have screens (Phase 5 and 6)
        public static readonly QuestionFormat[] OffenseFormats = { QuestionFormat.Strike };

        //Returns null and says why when the stage can't be played
        public static StagePlan Build(ContentDatabase database, string stageId, out string problem)
        {
            problem = null;
            if (database == null)
            {
                problem = "There's no content database yet.";
                return null;
            }

            StageData stage = database.GetStage(stageId);
            if (stage == null)
            {
                problem = "There's no stage called '" + stageId + "'.";
                return null;
            }

            var plan = new StagePlan { stage = stage };

            foreach (string enemyId in stage.waves)
            {
                EnemyData enemy = database.GetEnemy(enemyId);
                if (enemy == null)
                {
                    problem = "Stage " + stage.id + " has an enemy nobody made: '" + enemyId + "'.";
                    return null;
                }
                plan.waves.Add(enemy);
            }

            if (plan.waves.Count == 0)
            {
                problem = "Stage " + stage.id + " has no waves.";
                return null;
            }

            foreach (int lesson in stage.lessons)
            {
                QuestionSet set = database.GetLesson(lesson);
                if (set == null) continue;

                foreach (QuestionData question in set.questions)
                {
                    if (Array.IndexOf(OffenseFormats, question.format) >= 0) plan.questions.Add(question);
                    else if (question.format == QuestionFormat.Parry) plan.parryQuestions.Add(question);
                }
            }

            if (plan.questions.Count == 0)
            {
                problem = "Stage " + stage.id + " has no strike questions in lesson " + string.Join(", ", stage.lessons) + ".";
                return null;
            }

            return plan;
        }
    }
}
