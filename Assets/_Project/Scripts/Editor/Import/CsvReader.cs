using System.Collections.Generic;
using System.Text;

namespace JLPTQuest.EditorTools
{
    /*
        Plain CSV reader, the same flavour Google Sheets exports.
        Handles quoted cells, "" inside quotes, commas and line breaks inside quotes, CRLF and a leading BOM.
    */
    public static class CsvReader
    {
        public static List<List<string>> Parse(string text)
        {
            var rows = new List<List<string>>();
            if (string.IsNullOrEmpty(text)) return rows;

            text = text.Replace("\r\n", "\n").Replace('\r', '\n');
            int start = text[0] == '﻿' ? 1 : 0; //Skip the BOM

            var row = new List<string>();
            var cell = new StringBuilder();
            bool inQuotes = false;

            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c != '"')
                    {
                        cell.Append(c);
                        continue;
                    }

                    bool escaped = i + 1 < text.Length && text[i + 1] == '"';
                    if (escaped)
                    {
                        cell.Append('"');
                        i++;
                    }
                    else inQuotes = false;
                    continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        row.Add(cell.ToString());
                        cell.Clear();
                        break;
                    case '\n':
                        row.Add(cell.ToString());
                        cell.Clear();
                        rows.Add(row);
                        row = new List<string>();
                        break;
                    default:
                        cell.Append(c);
                        break;
                }
            }

            //The last line might not end with a line break
            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }

            return rows;
        }
    }
}
