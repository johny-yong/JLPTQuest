using JLPTQuest.EditorTools;
using NUnit.Framework;

namespace JLPTQuest.Tests
{
    public class CsvReaderTests
    {
        [Test]
        public void QuotedCellKeepsCommasAndLineBreaks()
        {
            var rows = CsvReader.Parse("a,\"b,c\",\"d\ne\"\n1,2,3");

            Assert.AreEqual(2, rows.Count);
            CollectionAssert.AreEqual(new[] { "a", "b,c", "d\ne" }, rows[0]);
            CollectionAssert.AreEqual(new[] { "1", "2", "3" }, rows[1]);
        }

        [Test]
        public void DoubledQuotesBecomeOneQuote()
        {
            var rows = CsvReader.Parse("\"say \"\"hi\"\"\"");
            Assert.AreEqual("say \"hi\"", rows[0][0]);
        }

        [Test]
        public void BomAndWindowsLineEndingsAreHandled()
        {
            var rows = CsvReader.Parse("﻿id,lesson\r\nL01-001,1\r\n");

            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("id", rows[0][0]);
            Assert.AreEqual("1", rows[1][1]);
        }

        [Test]
        public void EmptyCellsStayInPlace()
        {
            var rows = CsvReader.Parse("a,,c");
            CollectionAssert.AreEqual(new[] { "a", "", "c" }, rows[0]);
        }
    }
}
