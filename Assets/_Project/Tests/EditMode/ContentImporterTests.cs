using JLPTQuest.EditorTools;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class ContentImporterTests
    {
        private const string questionHeader = "id,lesson,wordClass,format,prompt,instruction,answer,wrongChoices,rule,english,tags,status,notes\n";
        private const string enemies = "id,name,nameJp,kind,hits,attack,attackCd,skill,face,wordSeal,status,notes\nkuchi-he,Kuchi-he,くちへ,grunt,3,20,3,none,令令|も|へ,{先生|せんせい},ready,\n";
        private const string stages = "id,realm,floor,title,lessons,waves,status,notes\nR1-F1,1,1,Test floor,1,kuchi-he,ready,\n";

        private static ContentBundle Import(string questionRows, out ImportReport report, string stageCsv = stages)
        {
            report = new ImportReport();
            return ContentImporter.Build(questionHeader + questionRows, enemies, stageCsv, report);
        }

        private static string Row(string id, string format, string prompt, string answer, string wrong, string status = "ready")
        {
            return id + ",1,particle," + format + "," + prompt + ",," + answer + "," + wrong + ",A rule.,An English line.,,"+ status + ",\n";
        }

        [Test]
        public void MissingEnglishIsFlagged()
        {
            string row = "L01-001,1,particle,strike,わたし＿ {学生|がくせい}です。,,は,を|に|で,A rule.,,,ready,\n";
            ContentBundle bundle = Import(row, out ImportReport report);
            Assert.AreEqual(1, bundle.questions.Count, "Still imports, it's a warning");
            Assert.AreEqual(1, report.WarningCount);
            Assert.AreEqual("english", report.issues[0].column);
        }

        [Test]
        public void ParryWithoutFactsIsFlagged()
        {
            ContentBundle bundle = Import(Row("L01-026", "parry", "おなまえは？", "アンナです。", "アンナさんです。|アンナですか。|いいえ、アンナです。"), out ImportReport report);
            Assert.AreEqual(1, bundle.questions.Count, "Still imports, it's a warning");
            Assert.AreEqual(1, report.WarningCount);
            StringAssert.Contains("facts", report.issues[0].message);

            string withFacts = "L01-026,1,expression,parry,おなまえは？,\"you: Anna, 28, student\",アンナです。,アンナさんです。|アンナですか。|いいえ、アンナです。,A rule.,An English line.,,ready,\n";
            Import(withFacts, out ImportReport clean);
            Assert.AreEqual(0, clean.WarningCount, string.Join("\n", clean.issues));
        }

        [Test]
        public void GoodStrikeRowImports()
        {
            ContentBundle bundle = Import(Row("L01-001", "strike", "わたし＿ {学生|がくせい}です。", "は", "を|に|で"), out ImportReport report);

            Assert.AreEqual(0, report.ErrorCount, string.Join("\n", report.issues));
            Assert.AreEqual(1, bundle.questions.Count);
            Assert.AreEqual("は", bundle.questions[0].slots[0].MainAnswer);
            Assert.AreEqual(3, bundle.questions[0].slots[0].wrong.Count);
            Assert.AreEqual(1, bundle.stages.Count);
            Assert.AreEqual("令令", bundle.enemies[0].faceEyes);
        }

        [Test]
        public void WrongChoiceThatIsAlsoRightIsCaught()
        {
            ContentBundle bundle = Import(Row("L05-001", "strike", "{京都|きょうと}＿ {行|い}きます。", "へ|に", "に|で|と"), out ImportReport report);

            Assert.AreEqual(0, bundle.questions.Count);
            StringAssert.Contains("also a right answer", report.issues[0].message);
            Assert.AreEqual(2, report.issues[0].row);
        }

        [Test]
        public void StrikeNeedsExactlyOneBlank()
        {
            Import(Row("L01-001", "strike", "わたしは がくせいです。", "は", "を|に|で"), out ImportReport report);
            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        public void MultiNeedsOneAnswerPerBlank()
        {
            Import(Row("L01-014", "multi", "わたし＿ さくら＿ がくせいです。", "は", "を|に|で"), out ImportReport report);
            Assert.Greater(report.ErrorCount, 0);
        }

        [Test]
        public void GoodMultiRowImports()
        {
            ContentBundle bundle = Import(Row("L01-014", "multi", "わたし＿ さくら＿ がくせいです。", "は ; の", "を|に|で ; を|に|へ"), out ImportReport report);

            Assert.AreEqual(0, report.ErrorCount, string.Join("\n", report.issues));
            Assert.AreEqual(2, bundle.questions[0].slots.Count);
            Assert.AreEqual("の", bundle.questions[0].slots[1].MainAnswer);
        }

        [Test]
        public void TraceAnswersMustBeKana()
        {
            Import(Row("L05-020", "trace", "＿へ いきます。", "駅", ""), out ImportReport bad);
            Assert.AreEqual(1, bad.ErrorCount);

            Import(Row("L05-020", "trace", "＿へ いきます。", "{駅|えき}", ""), out ImportReport good);
            Assert.AreEqual(0, good.ErrorCount, string.Join("\n", good.issues));
        }

        [Test]
        public void BuildOrdersMustUseTheSameChunks()
        {
            Import(Row("L04-026", "build", "I'm a student.", "わたしは / がくせいです|がくせいです / わたしは", ""), out ImportReport good);
            Assert.AreEqual(0, good.ErrorCount, string.Join("\n", good.issues));

            Import(Row("L04-026", "build", "I'm a student.", "わたしは / がくせいです|わたしは / せんせいです", ""), out ImportReport bad);
            Assert.AreEqual(1, bad.ErrorCount);
        }

        [Test]
        public void DuplicateIdsAreCaught()
        {
            string rows = Row("L01-001", "strike", "わたし＿ がくせいです。", "は", "を|に|で") + Row("L01-001", "strike", "あなた＿ がくせいです。", "は", "を|に|で");
            ContentBundle bundle = Import(rows, out ImportReport report);

            Assert.AreEqual(1, bundle.questions.Count);
            StringAssert.Contains("already used on row 2", report.issues[0].message);
        }

        [Test]
        public void DraftsAndRetiredRowsAreLeftOut()
        {
            string rows = Row("L01-001", "strike", "わたし＿ がくせいです。", "は", "を|に|で", "draft")
                + Row("L01-002", "strike", "あなた＿ がくせいです。", "は", "を|に|で", "retired");
            ContentBundle bundle = Import(rows, out ImportReport report);

            Assert.AreEqual(0, bundle.questions.Count);
            Assert.AreEqual(1, report.draftsSkipped);
            Assert.AreEqual(1, report.retiredSkipped);
            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        public void FullWidthSeparatorsStillWork()
        {
            ContentBundle bundle = Import(Row("L05-001", "strike", "{京都|きょうと}＿ {行|い}きます。", "へ｜に", "で｜と｜の"), out ImportReport report);

            Assert.AreEqual(0, report.ErrorCount, string.Join("\n", report.issues));
            Assert.AreEqual(2, bundle.questions[0].slots[0].accepted.Count);
        }

        [Test]
        public void QuotedCommasSurviveTheTrip()
        {
            string row = "L01-027,1,expression,parry,がくせいですか。,,\"はい、がくせいです。\",いいえ、がくせいです。|がくせいですか。|はい、ちがいます。,A rule.,\"Are you a student? Yes, I am.\",,ready,\n";
            ContentBundle bundle = Import(row, out ImportReport report);

            Assert.AreEqual(0, report.ErrorCount, string.Join("\n", report.issues));
            Assert.AreEqual("Are you a student? Yes, I am.", bundle.questions[0].english);
        }

        [Test]
        public void StageWithUnknownEnemyIsDropped()
        {
            string stageCsv = "id,realm,floor,title,lessons,waves,status,notes\nR1-F1,1,1,Test,1,kuchi-he|ghost,ready,\n";
            ContentBundle bundle = Import(Row("L01-001", "strike", "わたし＿ がくせいです。", "は", "を|に|で"), out ImportReport report, stageCsv);

            Assert.AreEqual(0, bundle.stages.Count);
            StringAssert.Contains("'ghost'", report.issues[0].message);
        }

        [Test]
        public void MissingColumnStopsEverything()
        {
            var report = new ImportReport();
            ContentImporter.Build("id,lesson,wordClass,format,prompt,status\n", enemies, stages, report);

            Assert.IsTrue(report.aborted);
        }
    }
}
