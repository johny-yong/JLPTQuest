using System;
using System.Collections.Generic;
using System.Text;

namespace JLPTQuest
{
    public enum KanaScript { Hiragana, Katakana }

    //One square on the chart. Blank ones keep the gaps in the や and わ rows lined up
    public readonly struct KanaEntry
    {
        public readonly string kana; //Hiragana, or katakana in a katakana-only section
        public readonly string romaji;

        public KanaEntry(string kana, string romaji)
        {
            this.kana = kana;
            this.romaji = romaji;
        }

        public bool IsBlank => string.IsNullOrEmpty(kana);
        public string Katakana => KanaChart.ToKatakana(kana);

        public string In(KanaScript script)
        {
            return script == KanaScript.Katakana ? Katakana : kana;
        }
    }

    //One block of the chart, drawn as rows of columns squares
    public class KanaSection
    {
        public readonly string title; //Written with hiragana, it's switched over for the katakana chart
        public readonly int columns;
        public readonly bool katakanaOnly;
        public readonly KanaEntry[] entries;
        private readonly string hiraganaNote;
        private readonly string katakanaNote;

        public KanaSection(string title, int columns, string kana, string romaji,
            string hiraganaNote = null, string katakanaNote = null, bool katakanaOnly = false)
        {
            this.title = title;
            this.columns = columns;
            this.katakanaOnly = katakanaOnly;
            this.hiraganaNote = hiraganaNote;
            this.katakanaNote = katakanaNote;
            entries = Parse(kana, romaji);
        }

        public int Rows => entries.Length / columns;

        public bool ShowsIn(KanaScript script)
        {
            return script == KanaScript.Katakana || !katakanaOnly;
        }

        public string Title(KanaScript script)
        {
            return script == KanaScript.Katakana ? KanaChart.ToKatakana(title) : title;
        }

        public string Note(KanaScript script)
        {
            return script == KanaScript.Katakana ? katakanaNote : hiraganaNote;
        }

        //Both strings are space separated and line up one to one, _ is a blank square
        private static KanaEntry[] Parse(string kana, string romaji)
        {
            string[] kanaParts = kana.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] romajiParts = romaji.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (kanaParts.Length != romajiParts.Length)
                throw new ArgumentException("Kana and romaji don't line up: " + kanaParts.Length + " vs " + romajiParts.Length);

            var entries = new KanaEntry[kanaParts.Length];
            for (int i = 0; i < kanaParts.Length; i++)
            {
                bool blank = kanaParts[i] == "_";
                entries[i] = new KanaEntry(blank ? "" : kanaParts[i], blank ? "" : romajiParts[i]);
            }
            return entries;
        }
    }

    /*
        The kana chart the Codex shows, with Hepburn romaji under everything.
        It's written once in hiragana and turned into katakana by shifting the code points, so the two can't drift apart.
        Same counts as Realm 0 plans for: 46 basic, 25 voiced, 33 combos. Katakana adds the loanword sounds on top.
    */
    public static class KanaChart
    {
        public static readonly KanaSection[] Sections =
        {
            new KanaSection("BASIC SOUNDS", 5,
                "あ い う え お か き く け こ さ し す せ そ た ち つ て と な に ぬ ね の " +
                "は ひ ふ へ ほ ま み む め も や _ ゆ _ よ ら り る れ ろ わ _ _ _ を ん _ _ _ _",
                "a i u e o ka ki ku ke ko sa shi su se so ta chi tsu te to na ni nu ne no " +
                "ha hi fu he ho ma mi mu me mo ya _ yu _ yo ra ri ru re ro wa _ _ _ wo n _ _ _ _"),

            new KanaSection("VOICED SOUNDS", 5,
                "が ぎ ぐ げ ご ざ じ ず ぜ ぞ だ ぢ づ で ど ば び ぶ べ ぼ ぱ ぴ ぷ ぺ ぽ",
                "ga gi gu ge go za ji zu ze zo da ji zu de do ba bi bu be bo pa pi pu pe po"),

            new KanaSection("COMBOS WITH SMALL ゃ ゅ ょ", 3,
                "きゃ きゅ きょ しゃ しゅ しょ ちゃ ちゅ ちょ にゃ にゅ にょ ひゃ ひゅ ひょ みゃ みゅ みょ " +
                "りゃ りゅ りょ ぎゃ ぎゅ ぎょ じゃ じゅ じょ びゃ びゅ びょ ぴゃ ぴゅ ぴょ",
                "kya kyu kyo sha shu sho cha chu cho nya nyu nyo hya hyu hyo mya myu myo " +
                "rya ryu ryo gya gyu gyo ja ju jo bya byu byo pya pyu pyo",
                hiraganaNote: "A small っ doubles the next sound: きって is kitte.",
                katakanaNote: "A small ッ doubles the next sound, and ー makes a vowel long: ベッド is beddo, コーヒー is koohii."),

            new KanaSection("LOANWORD SOUNDS", 3,
                "ファ フィ フェ フォ ティ ディ シェ ジェ チェ ウィ ウェ ウォ",
                "fa fi fe fo ti di she je che wi we wo",
                katakanaNote: "Katakana only, for sounds Japanese borrowed: フォーク is fooku.",
                katakanaOnly: true),
        };

        private const char FirstHiragana = 'ぁ'; //ぁ
        private const char LastHiragana = 'ゖ'; //ゖ
        private const int ToKatakanaShift = 0x60; //ぁ to ァ

        //Hiragana becomes katakana, everything else (ー, katakana, latin) passes through as it is
        public static string ToKatakana(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
                sb.Append(c >= FirstHiragana && c <= LastHiragana ? (char)(c + ToKatakanaShift) : c);
            return sb.ToString();
        }

        public static IEnumerable<KanaSection> For(KanaScript script)
        {
            foreach (KanaSection section in Sections)
                if (section.ShowsIn(script)) yield return section;
        }

        //How many real squares the chart has, blanks don't count
        public static int Count(KanaScript script)
        {
            int count = 0;
            foreach (KanaSection section in For(script))
                foreach (KanaEntry entry in section.entries)
                    if (!entry.IsBlank) count++;
            return count;
        }
    }
}
