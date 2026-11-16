using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JLPTQuest.EditorTools
{
    public enum IssueLevel { Warning, Error }

    public class ImportIssue
    {
        public IssueLevel level;
        public string tab;
        public int row; //0 means the whole tab
        public string column;
        public string message;

        public override string ToString()
        {
            string where = row > 0 ? tab + " row " + row : tab;
            string what = string.IsNullOrEmpty(column) ? string.Empty : column + ": ";
            return "[" + where + "] " + what + message;
        }
    }

    //Everything that went wrong or looked off, with tab and row so whoever wrote it can find it fast
    public class ImportReport
    {
        public readonly List<ImportIssue> issues = new List<ImportIssue>();
        public bool aborted; //Something so broken nothing got written, like a missing tab or column
        public int questionsImported;
        public int lessonsImported;
        public int enemiesImported;
        public int stagesImported;
        public int draftsSkipped;
        public int retiredSkipped;

        private readonly HashSet<string> failedRows = new HashSet<string>();

        public int ErrorCount => Count(IssueLevel.Error);
        public int WarningCount => Count(IssueLevel.Warning);
        public int RowsSkippedForErrors => failedRows.Count;

        public void Error(string tab, int row, string column, string message)
        {
            Add(IssueLevel.Error, tab, row, column, message);
            if (row > 0) failedRows.Add(tab + "#" + row);
        }

        public void Warn(string tab, int row, string column, string message)
        {
            Add(IssueLevel.Warning, tab, row, column, message);
        }

        public void Abort(string tab, string message)
        {
            Add(IssueLevel.Error, tab, 0, null, message);
            aborted = true;
        }

        public string Summary()
        {
            if (aborted)
                return "Import stopped before writing anything (" + ErrorCount + " problem(s)). Nothing in the game changed.";

            var builder = new StringBuilder();
            builder.Append("Imported ").Append(questionsImported).Append(" questions across ").Append(lessonsImported)
                .Append(" lessons, ").Append(enemiesImported).Append(" enemies and ").Append(stagesImported).Append(" stages.");
            if (RowsSkippedForErrors > 0) builder.Append(" Skipped ").Append(RowsSkippedForErrors).Append(" row(s) with errors.");
            if (draftsSkipped > 0) builder.Append(" Left out ").Append(draftsSkipped).Append(" draft(s).");
            if (retiredSkipped > 0) builder.Append(" Left out ").Append(retiredSkipped).Append(" retired.");
            builder.Append(" ").Append(ErrorCount).Append(" error(s), ").Append(WarningCount).Append(" warning(s).");
            return builder.ToString();
        }

        public void LogToConsole()
        {
            foreach (ImportIssue issue in issues)
            {
                if (issue.level == IssueLevel.Error) Debug.LogError("[Content] " + issue);
                else Debug.LogWarning("[Content] " + issue);
            }
            Debug.Log("[Content] " + Summary());
        }

        private void Add(IssueLevel level, string tab, int row, string column, string message)
        {
            issues.Add(new ImportIssue { level = level, tab = tab, row = row, column = column, message = message });
        }

        private int Count(IssueLevel level)
        {
            int count = 0;
            foreach (ImportIssue issue in issues)
                if (issue.level == level) count++;
            return count;
        }
    }
}
