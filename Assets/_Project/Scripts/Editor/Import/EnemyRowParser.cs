using System.Collections.Generic;

namespace JLPTQuest.EditorTools
{
    //Turns one row of the Enemies tab into EnemyData
    public static class EnemyRowParser
    {
        public const string Tab = "Enemies";
        public static readonly string[] requiredColumns = { "id", "name", "kind", "hits", "attack", "attackCd", "skill", "status" };

        public static EnemyData Parse(CsvRow row, ImportReport report)
        {
            int errorsBefore = report.ErrorCount;
            int line = row.sheetRow;

            var enemy = new EnemyData
            {
                id = row.Get("id"),
                sheetRow = line,
                displayName = row.Get("name"),
                nameJp = row.Get("nameJp"),
                kind = RowHelpers.ReadEnum(row, "kind", Tab, report, EnemyKind.Grunt, false),
                hits = RowHelpers.ReadInt(row, "hits", 1, 99, Tab, report),
                attack = RowHelpers.ReadInt(row, "attack", 1, 999, Tab, report),
                attackCd = RowHelpers.ReadInt(row, "attackCd", 1, 9, Tab, report),
                skill = RowHelpers.ReadEnum(row, "skill", Tab, report, EnemySkill.None, true),
                wordSeal = RowHelpers.ReadMarkup(row, "wordSeal", false, Tab, report)
            };

            if (enemy.displayName.Length == 0) report.Error(Tab, line, "name", "empty");

            //Face is eyes | nose | mouth, a missing part just stays blank
            string face = ContentText.NormaliseSymbols(row.Get("face"));
            List<string> parts = ContentText.SplitOutsideBraces(face, ContentText.OrSeparator);
            if (face.Length > 0 && parts.Count != 3) report.Warn(Tab, line, "face", "should be eyes | nose | mouth, found " + parts.Count + " part(s)");
            enemy.faceEyes = parts.Count > 0 ? parts[0] : string.Empty;
            enemy.faceNose = parts.Count > 1 ? parts[1] : string.Empty;
            enemy.faceMouth = parts.Count > 2 ? parts[2] : string.Empty;

            return report.ErrorCount == errorsBefore ? enemy : null;
        }
    }
}
