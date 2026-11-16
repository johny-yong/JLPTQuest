using System;
using System.Globalization;

namespace JLPTQuest.EditorTools
{
    public enum RowStatus { Ready, Draft, Retired }

    //Bits every tab shares: ids, numbers, dropdown values, status, furigana markup
    public static class RowHelpers
    {
        public static bool IsValidId(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            foreach (char c in id)
            {
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
                if (!ok) return false;
            }
            return true;
        }

        public static RowStatus ReadStatus(CsvRow row, string tab, ImportReport report)
        {
            string status = row.Get("status").ToLowerInvariant();
            switch (status)
            {
                case "ready": return RowStatus.Ready;
                case "draft": return RowStatus.Draft;
                case "retired": return RowStatus.Retired;
                case "":
                    report.Warn(tab, row.sheetRow, "status", "empty, so it's treated as a draft");
                    return RowStatus.Draft;
                default:
                    report.Warn(tab, row.sheetRow, "status", "'" + status + "' isn't ready, draft or retired, so it's treated as a draft");
                    return RowStatus.Draft;
            }
        }

        public static int ReadInt(CsvRow row, string column, int min, int max, string tab, ImportReport report)
        {
            string text = row.Get(column);
            if (text.Length == 0)
            {
                report.Error(tab, row.sheetRow, column, "empty, needs a whole number");
                return min;
            }

            //Sheets sometimes hands back 3.0 for a 3
            bool parsed = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number);
            if (!parsed || Math.Abs(number - Math.Round(number)) > 0.0001)
            {
                report.Error(tab, row.sheetRow, column, "'" + text + "' isn't a whole number");
                return min;
            }

            int value = (int)Math.Round(number);
            if (value < min || value > max)
            {
                report.Error(tab, row.sheetRow, column, value + " is outside " + min + " to " + max);
                return min;
            }
            return value;
        }

        public static T ReadEnum<T>(CsvRow row, string column, string tab, ImportReport report, T fallback, bool allowEmpty) where T : struct, Enum
        {
            string text = row.Get(column);
            if (text.Length == 0)
            {
                if (!allowEmpty) report.Error(tab, row.sheetRow, column, "empty, pick one from the dropdown");
                return fallback;
            }

            //Enum.TryParse happily takes numbers, which would let a typo like 3 slip through
            bool isNumber = char.IsDigit(text[0]) || text[0] == '-';
            if (!isNumber && Enum.TryParse(text, true, out T value)) return value;

            report.Error(tab, row.sheetRow, column, "'" + text + "' isn't one of: " + string.Join(", ", Enum.GetNames(typeof(T))));
            return fallback;
        }

        //Normalised text with its furigana markup checked
        public static string ReadMarkup(CsvRow row, string column, bool required, string tab, ImportReport report)
        {
            string text = ContentText.NormaliseSymbols(row.Get(column));
            if (text.Length == 0)
            {
                if (required) report.Error(tab, row.sheetRow, column, "empty");
                return text;
            }

            if (!RubyMarkup.TryValidate(text, out string problem)) report.Error(tab, row.sheetRow, column, problem);
            return text;
        }
    }
}
