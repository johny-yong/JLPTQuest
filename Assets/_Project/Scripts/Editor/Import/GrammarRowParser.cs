namespace JLPTQuest.EditorTools
{
    //Turns one row of the Grammar tab into GrammarData
    public static class GrammarRowParser
    {
        public const string Tab = "Grammar";
        public static readonly string[] requiredColumns = { "id", "lesson", "name", "pattern", "explanation", "status" };

        public static GrammarData Parse(CsvRow row, ImportReport report)
        {
            int errorsBefore = report.ErrorCount;

            var grammar = new GrammarData
            {
                id = row.Get("id"),
                sheetRow = row.sheetRow,
                lesson = RowHelpers.ReadInt(row, "lesson", 0, 50, Tab, report),
                name = RowHelpers.ReadMarkup(row, "name", true, Tab, report),
                pattern = RowHelpers.ReadMarkup(row, "pattern", true, Tab, report),
                explanation = RowHelpers.ReadMarkup(row, "explanation", true, Tab, report)
            };

            return report.ErrorCount == errorsBefore ? grammar : null;
        }
    }

    //Turns one row of the Words tab into WordData
    public static class WordRowParser
    {
        public const string Tab = "Words";
        public static readonly string[] requiredColumns = { "id", "lesson", "word", "meaning", "status" };

        public static WordData Parse(CsvRow row, ImportReport report)
        {
            int errorsBefore = report.ErrorCount;

            var word = new WordData
            {
                id = row.Get("id"),
                sheetRow = row.sheetRow,
                lesson = RowHelpers.ReadInt(row, "lesson", 0, 50, Tab, report),
                word = RowHelpers.ReadMarkup(row, "word", true, Tab, report),
                meaning = row.Get("meaning")
            };
            if (word.meaning.Length == 0) report.Error(Tab, row.sheetRow, "meaning", "empty, the scroll needs to say what the word means");

            return report.ErrorCount == errorsBefore ? word : null;
        }
    }
}
