using JLPTQuest.EditorTools;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class GrammarImportTests
    {
        private const string questionHeader = "id,lesson,wordClass,format,prompt,instruction,answer,wrongChoices,rule,english,tags,grammar,status,notes\n";
        private const string enemies = "id,name,nameJp,kind,hits,attack,attackCd,skill,face,wordSeal,status,notes\nkuchi-he,Kuchi-he,くちへ,grunt,3,20,3,none,令令|も|へ,{先生|せんせい},ready,\n";
        private const string stages = "id,realm,floor,title,lessons,waves,status,notes\nR1-F1,1,1,Test floor,1,kuchi-he,ready,\n";
        private const string grammarHeader = "id,lesson,name,pattern,explanation,status,notes\n";
        private const string wordHeader = "id,lesson,word,meaning,status,notes\n";

        private static string Question(string id, string grammar)
        {
            return id + ",1,particle,strike,わたし＿ {学生|がくせい}です。,,は,を|に|で,A rule.,I am a student.,," + grammar + ",ready,\n";
        }

        private static ContentBundle Import(string questionRows, string grammarRows, string wordRows, out ImportReport report)
        {
            report = new ImportReport();
            return ContentImporter.Build(questionHeader + questionRows, enemies, stages, grammarHeader + grammarRows, wordHeader + wordRows, report);
        }

        [Test]
        public void GrammarAndWordsImportInSheetOrder()
        {
            string grammar = "G01-wa,1,は: the topic,N1 は N2 です,は marks the topic.,ready,\nG01-mo,1,も: also,N1 も N2 です,も means also.,ready,\n";
            string words = "W01-01,1,{学生|がくせい},student,ready,\n";
            ContentBundle bundle = Import(Question("L01-001", "G01-wa"), grammar, words, out ImportReport report);

            Assert.AreEqual(0, report.ErrorCount + report.WarningCount, string.Join("\n", report.issues));
            Assert.AreEqual(2, bundle.grammar.Count);
            Assert.AreEqual("G01-mo", bundle.grammar[1].id);
            Assert.AreEqual("{学生|がくせい}", bundle.words[0].word);
            Assert.AreEqual("G01-wa", bundle.questions[0].grammar);
        }

        [Test]
        public void UnknownGrammarIdIsFlaggedAndDropped()
        {
            ContentBundle bundle = Import(Question("L01-001", "G01-nope"), "G01-wa,1,は,N は N です,Topic.,ready,\n", string.Empty, out ImportReport report);
            Assert.AreEqual(1, bundle.questions.Count, "The question still plays");
            Assert.AreEqual(1, report.WarningCount);
            Assert.AreEqual("grammar", report.issues[0].column);
            Assert.AreEqual(string.Empty, bundle.questions[0].grammar);
        }

        [Test]
        public void GrammarNeedsAnExplanation()
        {
            ContentBundle bundle = Import(Question("L01-001", ""), "G01-wa,1,は,N は N です,,ready,\n", string.Empty, out ImportReport report);
            Assert.AreEqual(0, bundle.grammar.Count);
            Assert.AreEqual(1, report.ErrorCount);
            Assert.AreEqual("explanation", report.issues[0].column);
        }

        [Test]
        public void WordNeedsAMeaning()
        {
            ContentBundle bundle = Import(Question("L01-001", ""), string.Empty, "W01-01,1,{学生|がくせい},,ready,\n", out ImportReport report);
            Assert.AreEqual(0, bundle.words.Count);
            Assert.AreEqual(1, report.ErrorCount);
        }

        [Test]
        public void DraftGrammarStaysOut()
        {
            ContentBundle bundle = Import(Question("L01-001", ""), "G01-wa,1,は,N は N です,Topic.,draft,\n", string.Empty, out ImportReport report);
            Assert.AreEqual(0, bundle.grammar.Count);
            Assert.AreEqual(1, report.draftsSkipped);
        }
    }
}
