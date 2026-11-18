using System.Collections.Generic;

namespace JLPTQuest.EditorTools
{
    //Turns one row of the Questions tab into QuestionData, or says exactly what's wrong with it
    public static class QuestionRowParser
    {
        public const string Tab = "Questions";
        public static readonly string[] requiredColumns = { "id", "lesson", "wordClass", "format", "prompt", "answer", "wrongChoices", "rule", "status" };

        private const int traceWarnLength = 8;
        private const int traceMaxLength = 12; //Anything longer won't fit a path on the 5x6 board

        //Returns null when the row has errors, the report says why
        public static QuestionData Parse(CsvRow row, ImportReport report)
        {
            int errorsBefore = report.ErrorCount;
            int line = row.sheetRow;

            var question = new QuestionData
            {
                id = row.Get("id"),
                sheetRow = line,
                lesson = RowHelpers.ReadInt(row, "lesson", 0, 50, Tab, report),
                wordClass = RowHelpers.ReadEnum(row, "wordClass", Tab, report, WordClass.Noun, false),
                format = RowHelpers.ReadEnum(row, "format", Tab, report, QuestionFormat.Strike, false),
                prompt = RowHelpers.ReadMarkup(row, "prompt", true, Tab, report),
                instruction = row.Get("instruction"),
                rule = row.Get("rule"),
                english = row.Get("english"),
                grammar = row.Get("grammar"),
                tags = ContentText.SplitOutsideBraces(row.Get("tags"), ContentText.OrSeparator)
            };

            if (question.rule.Length == 0)
                report.Warn(Tab, line, "rule", "empty, so the teach card has nothing to say after a miss");

            if (question.english.Length == 0)
                report.Warn(Tab, line, "english", "empty, so the teach card can't show what the sentence means");

            //"Is Ken an engineer?" can't be answered without knowing Ken
            if (question.format == QuestionFormat.Parry && question.instruction.Length == 0)
                report.Warn(Tab, line, "instruction", "a parry needs the facts that make one reply true, like: you: Anna, 28, student");

            question.slots = ReadSlots(row, report);

            //Format checks only make sense once the basic fields read fine
            if (report.ErrorCount == errorsBefore) CheckFormat(question, report);
            return report.ErrorCount == errorsBefore ? question : null;
        }

        private static List<AnswerSlot> ReadSlots(CsvRow row, ImportReport report)
        {
            var slots = new List<AnswerSlot>();
            string answerText = ContentText.NormaliseSymbols(row.Get("answer"));
            string wrongText = ContentText.NormaliseSymbols(row.Get("wrongChoices"));

            if (answerText.Length == 0)
            {
                report.Error(Tab, row.sheetRow, "answer", "empty");
                return slots;
            }

            List<string> answerGroups = ContentText.SplitOutsideBraces(answerText, ContentText.BlankSeparator);
            List<string> wrongGroups = ContentText.SplitOutsideBraces(wrongText, ContentText.BlankSeparator);

            if (wrongGroups.Count > answerGroups.Count)
                report.Error(Tab, row.sheetRow, "wrongChoices", "has " + wrongGroups.Count + " groups split by ; but answer only has " + answerGroups.Count);

            for (int i = 0; i < answerGroups.Count; i++)
            {
                var slot = new AnswerSlot
                {
                    accepted = Dedupe(ContentText.SplitOutsideBraces(answerGroups[i], ContentText.OrSeparator)),
                    wrong = i < wrongGroups.Count ? ContentText.SplitOutsideBraces(wrongGroups[i], ContentText.OrSeparator) : new List<string>()
                };

                CheckMarkup(slot.accepted, "answer", row, report);
                CheckMarkup(slot.wrong, "wrongChoices", row, report);
                slots.Add(slot);
            }

            return slots;
        }

        private static void CheckFormat(QuestionData question, ImportReport report)
        {
            int line = question.sheetRow;
            int blanks = question.BlankCount;
            int slotCount = question.slots.Count;

            switch (question.format)
            {
                case QuestionFormat.Strike:
                    if (blanks != 1) report.Error(Tab, line, "prompt", "strike needs exactly one ＿, found " + blanks);
                    NeedSlots(question, 1, report);
                    NeedWrong(question, 3, report);
                    break;

                case QuestionFormat.Multi:
                    if (blanks < 2 || blanks > 3) report.Error(Tab, line, "prompt", "multi needs 2 or 3 ＿, found " + blanks);
                    else if (slotCount != blanks) report.Error(Tab, line, "answer", "has " + slotCount + " answers split by ; but the prompt has " + blanks + " blanks");
                    NeedWrong(question, 3, report);
                    break;

                case QuestionFormat.Trace:
                    if (blanks > 1) report.Error(Tab, line, "prompt", "trace takes one ＿ at most, found " + blanks);
                    NeedSlots(question, 1, report);
                    CheckTraceAnswers(question, report);
                    break;

                case QuestionFormat.Forge:
                case QuestionFormat.Parry:
                    NeedSlots(question, 1, report);
                    NeedWrong(question, 3, report);
                    break;

                case QuestionFormat.Build:
                    NeedSlots(question, 1, report);
                    CheckBuildOrders(question, report);
                    return; //Decoy chunks have their own clash check in there

                case QuestionFormat.Sort:
                    NeedSlots(question, 1, report);
                    NeedWrong(question, 1, report);
                    break;
            }

            CheckClashes(question, report);
        }

        private static void NeedSlots(QuestionData question, int count, ImportReport report)
        {
            if (question.slots.Count != count)
                report.Error(Tab, question.sheetRow, "answer", question.format.ToString().ToLowerInvariant() + " takes " + count + " answer group, found " + question.slots.Count + " (only multi uses ;)");
        }

        private static void NeedWrong(QuestionData question, int minimum, ImportReport report)
        {
            for (int i = 0; i < question.slots.Count; i++)
            {
                int found = question.slots[i].wrong.Count;
                if (found >= minimum) continue;

                string which = question.slots.Count > 1 ? " for blank " + (i + 1) : string.Empty;
                report.Error(Tab, question.sheetRow, "wrongChoices", "needs at least " + minimum + " wrong choices" + which + ", found " + found);
            }
        }

        //A wrong choice that's also right would mark a correct player wrong, the worst bug this game can have
        private static void CheckClashes(QuestionData question, ImportReport report)
        {
            for (int i = 0; i < question.slots.Count; i++)
            {
                AnswerSlot slot = question.slots[i];
                var right = new HashSet<string>();
                foreach (string option in slot.accepted)
                    right.Add(ContentText.Comparable(option));

                var seen = new HashSet<string>();
                var kept = new List<string>();
                foreach (string option in slot.wrong)
                {
                    string comparable = ContentText.Comparable(option);
                    if (right.Contains(comparable))
                    {
                        report.Error(Tab, question.sheetRow, "wrongChoices", "'" + option + "' is also a right answer. Move it to answer or drop it");
                        continue;
                    }

                    if (!seen.Add(comparable))
                    {
                        report.Warn(Tab, question.sheetRow, "wrongChoices", "'" + option + "' is listed twice, kept one");
                        continue;
                    }
                    kept.Add(option);
                }
                slot.wrong = kept;
            }
        }

        private static void CheckTraceAnswers(QuestionData question, ImportReport report)
        {
            foreach (AnswerSlot slot in question.slots)
            {
                foreach (string option in slot.accepted)
                {
                    string reading = ContentText.Comparable(option);
                    if (!ContentText.IsKanaOnly(reading))
                    {
                        report.Error(Tab, question.sheetRow, "answer", "trace answers must be kana (furigana counts), '" + option + "' isn't");
                        continue;
                    }

                    if (reading.Length > traceMaxLength)
                        report.Error(Tab, question.sheetRow, "answer", "'" + option + "' is " + reading.Length + " kana, the board fits " + traceMaxLength + " at most");
                    else if (reading.Length > traceWarnLength)
                        report.Warn(Tab, question.sheetRow, "answer", "'" + option + "' is " + reading.Length + " kana, long for the board");
                }
            }
        }

        //Every valid order has to use the same chunks, otherwise the tray can't hold them all
        private static void CheckBuildOrders(QuestionData question, ImportReport report)
        {
            if (question.slots.Count == 0) return;
            AnswerSlot slot = question.slots[0];
            if (slot.accepted.Count == 0) return;

            List<string> firstChunks = ContentText.SplitOutsideBraces(slot.accepted[0], ContentText.ChunkSeparator);
            if (firstChunks.Count < 2)
            {
                report.Error(Tab, question.sheetRow, "answer", "build needs at least 2 chunks split by /");
                return;
            }

            string firstKey = ChunkKey(firstChunks);
            for (int i = 1; i < slot.accepted.Count; i++)
            {
                List<string> chunks = ContentText.SplitOutsideBraces(slot.accepted[i], ContentText.ChunkSeparator);
                if (ChunkKey(chunks) != firstKey)
                    report.Error(Tab, question.sheetRow, "answer", "order " + (i + 1) + " doesn't use the same chunks as the first order");
            }

            var chunkSet = new HashSet<string>();
            foreach (string chunk in firstChunks)
                chunkSet.Add(ContentText.Comparable(chunk));

            foreach (string decoy in slot.wrong)
                if (chunkSet.Contains(ContentText.Comparable(decoy)))
                    report.Error(Tab, question.sheetRow, "wrongChoices", "decoy '" + decoy + "' is also a chunk of the answer");
        }

        private static string ChunkKey(List<string> chunks)
        {
            var sorted = new List<string>();
            foreach (string chunk in chunks)
                sorted.Add(ContentText.Comparable(chunk));
            sorted.Sort(System.StringComparer.Ordinal);
            return string.Join("\n", sorted);
        }

        private static void CheckMarkup(List<string> options, string column, CsvRow row, ImportReport report)
        {
            foreach (string option in options)
                if (!RubyMarkup.TryValidate(option, out string problem))
                    report.Error(Tab, row.sheetRow, column, "'" + option + "': " + problem);
        }

        private static List<string> Dedupe(List<string> options)
        {
            var seen = new HashSet<string>();
            var kept = new List<string>();
            foreach (string option in options)
                if (seen.Add(option)) kept.Add(option);
            return kept;
        }
    }
}
