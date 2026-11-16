using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace JLPTQuest
{
    //A bit of text, with a reading if it's kanji written as {漢字|かんじ}
    public readonly struct RubySegment
    {
        public readonly string text;
        public readonly string reading;

        public RubySegment(string text, string reading)
        {
            this.text = text;
            this.reading = reading;
        }

        public bool HasReading => !string.IsNullOrEmpty(reading);
    }

    //Reads the {漢字|かんじ} furigana markup and turns it into TextMeshPro rich text
    public static class RubyMarkup
    {
        //Broken markup never throws, it just shows up as plain text. TryValidate is what catches it in the importer
        public static List<RubySegment> Parse(string markup)
        {
            var segments = new List<RubySegment>();
            if (string.IsNullOrEmpty(markup)) return segments;

            var plain = new StringBuilder();
            int i = 0;
            while (i < markup.Length)
            {
                if (markup[i] == '{' && TryReadGroup(markup, i, out string text, out string reading, out int end))
                {
                    FlushPlain(segments, plain);
                    segments.Add(new RubySegment(text, reading));
                    i = end + 1;
                    continue;
                }

                plain.Append(markup[i]);
                i++;
            }

            FlushPlain(segments, plain);
            return segments;
        }

        public static bool TryValidate(string markup, out string problem)
        {
            problem = null;
            if (string.IsNullOrEmpty(markup)) return true;

            int i = 0;
            while (i < markup.Length)
            {
                char c = markup[i];
                if (c == '}')
                {
                    problem = "there's a } with no { before it";
                    return false;
                }

                if (c != '{')
                {
                    i++;
                    continue;
                }

                int close = markup.IndexOf('}', i + 1);
                if (close < 0)
                {
                    problem = "a { is never closed with }";
                    return false;
                }

                string inside = markup.Substring(i + 1, close - i - 1);
                if (inside.IndexOf('{') >= 0)
                {
                    problem = "furigana can't be nested inside another {}";
                    return false;
                }

                int bar = inside.IndexOf('|');
                if (bar <= 0 || bar == inside.Length - 1 || inside.IndexOf('|', bar + 1) >= 0)
                {
                    problem = "{" + inside + "} should look like {漢字|かんじ}";
                    return false;
                }

                string reading = inside.Substring(bar + 1);
                if (!ContentText.IsKanaOnly(reading))
                {
                    problem = "the reading in {" + inside + "} should be kana only";
                    return false;
                }

                i = close + 1;
            }

            return true;
        }

        //先生 out of {先生|せんせい}
        public static string StripReadings(string markup)
        {
            var builder = new StringBuilder();
            foreach (RubySegment segment in Parse(markup))
                builder.Append(segment.text);
            return builder.ToString();
        }

        //せんせい out of {先生|せんせい}, handy for kana checks and the trace board
        public static string ToReading(string markup)
        {
            var builder = new StringBuilder();
            foreach (RubySegment segment in Parse(markup))
                builder.Append(segment.HasReading ? segment.reading : segment.text);
            return builder.ToString();
        }

        /*
            Builds TextMeshPro rich text with the reading floating over its kanji.
            measureEm gives the width of a string at full size, in em.
            Each kanji group goes: kanji, step back to its middle, reading at small size, step forward to the end.
            If the reading is wider than the kanji, the kanji gets padded so readings never overlap.
        */
        public static string ToRichText(string markup, Func<string, float> measureEm, float rubyScale, float rubyOffsetEm)
        {
            var builder = new StringBuilder();
            foreach (RubySegment segment in Parse(markup))
            {
                if (!segment.HasReading)
                {
                    builder.Append(segment.text);
                    continue;
                }

                float baseWidth = measureEm(segment.text);
                float rubyWidth = measureEm(segment.reading) * rubyScale;
                float pad = Math.Max(0f, (rubyWidth - baseWidth) * 0.5f);
                float fullWidth = baseWidth + pad * 2f;

                builder.Append("<nobr>");
                if (pad > 0f) AppendSpace(builder, pad);
                builder.Append(segment.text);
                if (pad > 0f) AppendSpace(builder, pad);
                AppendSpace(builder, -(fullWidth + rubyWidth) * 0.5f);
                builder.Append("<voffset=").Append(Em(rubyOffsetEm)).Append("em><size=")
                    .Append(Math.Round(rubyScale * 100f).ToString(CultureInfo.InvariantCulture)).Append("%>")
                    .Append(segment.reading).Append("</size></voffset>");
                AppendSpace(builder, (fullWidth - rubyWidth) * 0.5f);
                builder.Append("</nobr>");
            }
            return builder.ToString();
        }

        private static bool TryReadGroup(string markup, int open, out string text, out string reading, out int end)
        {
            text = reading = null;
            end = markup.IndexOf('}', open + 1);
            if (end < 0) return false;

            int bar = markup.IndexOf('|', open + 1);
            int nextOpen = markup.IndexOf('{', open + 1);
            if (bar < 0 || bar > end || (nextOpen >= 0 && nextOpen < end)) return false;
            if (bar == open + 1 || bar == end - 1) return false;

            text = markup.Substring(open + 1, bar - open - 1);
            reading = markup.Substring(bar + 1, end - bar - 1);
            return true;
        }

        private static void FlushPlain(List<RubySegment> segments, StringBuilder plain)
        {
            if (plain.Length == 0) return;
            segments.Add(new RubySegment(plain.ToString(), null));
            plain.Clear();
        }

        private static void AppendSpace(StringBuilder builder, float em)
        {
            builder.Append("<space=").Append(Em(em)).Append("em>");
        }

        //Always a dot for decimals, a comma locale would break the tags
        private static string Em(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
