using System.Collections.Generic;

namespace JLPTQuest.EditorTools
{
    //A CSV with a header row. Cells are looked up by column name, so editors can reorder or add columns freely
    public class CsvTable
    {
        public readonly string name;
        public readonly List<string> headers = new List<string>();
        public readonly List<CsvRow> rows = new List<CsvRow>();
        private readonly Dictionary<string, int> columnIndex = new Dictionary<string, int>();

        public CsvTable(string name, string csvText)
        {
            this.name = name;
            List<List<string>> lines = CsvReader.Parse(csvText);
            if (lines.Count == 0) return;

            for (int i = 0; i < lines[0].Count; i++)
            {
                string header = lines[0][i].Trim();
                headers.Add(header);
                string key = Key(header);
                if (key.Length > 0 && !columnIndex.ContainsKey(key)) columnIndex[key] = i;
            }

            //Sheet row numbers start at 1 and the header takes row 1, so data starts at 2
            for (int i = 1; i < lines.Count; i++)
                rows.Add(new CsvRow(this, lines[i], i + 1));
        }

        public bool HasColumn(string column)
        {
            return columnIndex.ContainsKey(Key(column));
        }

        public List<string> MissingColumns(IEnumerable<string> required)
        {
            var missing = new List<string>();
            foreach (string column in required)
                if (!HasColumn(column)) missing.Add(column);
            return missing;
        }

        internal int IndexOf(string column)
        {
            return columnIndex.TryGetValue(Key(column), out int index) ? index : -1;
        }

        //Case and spaces don't matter, so "Word Class" still finds wordClass
        private static string Key(string header)
        {
            return header.Replace(" ", string.Empty).ToLowerInvariant();
        }
    }

    public class CsvRow
    {
        public readonly int sheetRow;
        private readonly CsvTable table;
        private readonly List<string> cells;

        public CsvRow(CsvTable table, List<string> cells, int sheetRow)
        {
            this.table = table;
            this.cells = cells;
            this.sheetRow = sheetRow;
        }

        //Trimmed cell text, or empty when the column or cell isn't there
        public string Get(string column)
        {
            int index = table.IndexOf(column);
            if (index < 0 || index >= cells.Count) return string.Empty;
            return cells[index].Trim();
        }

        public bool IsEmpty
        {
            get
            {
                foreach (string cell in cells)
                    if (!string.IsNullOrWhiteSpace(cell)) return false;
                return true;
            }
        }
    }
}
