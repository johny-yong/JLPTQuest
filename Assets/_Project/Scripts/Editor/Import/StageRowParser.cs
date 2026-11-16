using System.Globalization;

namespace JLPTQuest.EditorTools
{
    //Turns one row of the Stages tab into StageData. Enemy ids get checked later, once every enemy is read
    public static class StageRowParser
    {
        public const string Tab = "Stages";
        public static readonly string[] requiredColumns = { "id", "realm", "floor", "lessons", "waves", "status" };

        public static StageData Parse(CsvRow row, ImportReport report)
        {
            int errorsBefore = report.ErrorCount;
            int line = row.sheetRow;

            var stage = new StageData
            {
                id = row.Get("id"),
                sheetRow = line,
                realm = RowHelpers.ReadInt(row, "realm", 0, 99, Tab, report),
                floor = RowHelpers.ReadInt(row, "floor", 0, 99, Tab, report),
                title = row.Get("title")
            };

            foreach (string text in ContentText.SplitOutsideBraces(ContentText.NormaliseSymbols(row.Get("lessons")), ContentText.OrSeparator))
            {
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int lesson) && lesson >= 0 && lesson <= 50)
                    stage.lessons.Add(lesson);
                else
                    report.Error(Tab, line, "lessons", "'" + text + "' isn't a lesson number from 0 to 50");
            }
            if (stage.lessons.Count == 0) report.Error(Tab, line, "lessons", "needs at least one lesson, split by |");

            stage.waves = ContentText.SplitOutsideBraces(ContentText.NormaliseSymbols(row.Get("waves")), ContentText.OrSeparator);
            if (stage.waves.Count == 0) report.Error(Tab, line, "waves", "needs at least one enemy id, split by |");

            return report.ErrorCount == errorsBefore ? stage : null;
        }
    }
}
