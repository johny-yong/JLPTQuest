using System.Collections.Generic;
using System.Text;

namespace JLPTQuest
{
    /*
        The little symbol language used in the sheet, in one place so the importer and the game agree.
          ＿          a blank (typing _ works too)
          {漢字|かんじ}  kanji with furigana
          |           or, splits right answers or wrong choices
          ;           next blank
          /           next chunk, for the build format
    */
    public static class ContentText
    {
        public const char Blank = '＿';
        public const char OrSeparator = '|';
        public const char BlankSeparator = ';';
        public const char ChunkSeparator = '/';

        //Swaps full-width symbols for the normal ones and squashes any run of underscores into one blank
        public static string NormaliseSymbols(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var builder = new StringBuilder(text.Length);
            bool lastWasBlank = false;
            foreach (char raw in text)
            {
                char c = raw;
                switch (c)
                {
                    case '｜': c = '|'; break;
                    case '；': c = ';'; break;
                    case '／': c = '/'; break;
                    case '｛': c = '{'; break;
                    case '｝': c = '}'; break;
                }

                bool isBlank = c == '_' || c == Blank;
                if (isBlank)
                {
                    if (!lastWasBlank) builder.Append(Blank);
                    lastWasBlank = true;
                    continue;
                }

                lastWasBlank = false;
                builder.Append(c);
            }

            return builder.ToString().Trim();
        }

        public static int CountBlanks(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int count = 0;
            foreach (char c in text)
                if (c == Blank) count++;
            return count;
        }

        //Splits on a separator but leaves anything inside {} alone, so {何|なん}|なに splits into two not three
        public static List<string> SplitOutsideBraces(string text, char separator)
        {
            var parts = new List<string>();
            if (string.IsNullOrEmpty(text)) return parts;

            var current = new StringBuilder();
            int depth = 0;
            foreach (char c in text)
            {
                if (c == '{') depth++;
                else if (c == '}' && depth > 0) depth--;

                if (c == separator && depth == 0)
                {
                    AddIfNotEmpty(parts, current);
                    continue;
                }
                current.Append(c);
            }
            AddIfNotEmpty(parts, current);
            return parts;
        }

        //Kana only: hiragana, katakana, the long vowel mark, small kana. No kanji, latin or punctuation
        public static bool IsKanaOnly(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                bool hiragana = c >= 'ぁ' && c <= 'ゟ';
                bool katakana = c >= 'ァ' && c <= 'ヿ' && c != '・';
                if (!hiragana && !katakana) return false;
            }
            return true;
        }

        //For comparing answers: readings instead of kanji, no spaces at all
        public static string Comparable(string text)
        {
            string reading = RubyMarkup.ToReading(NormaliseSymbols(text));
            var builder = new StringBuilder(reading.Length);
            foreach (char c in reading)
                if (!char.IsWhiteSpace(c)) builder.Append(c);
            return builder.ToString();
        }

        private static void AddIfNotEmpty(List<string> parts, StringBuilder current)
        {
            string part = current.ToString().Trim();
            if (part.Length > 0) parts.Add(part);
            current.Clear();
        }
    }
}
