using System.Collections.Generic;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using terminalFr8Core.Activities;

namespace terminalTests.Unit
{
    [TestFixture, Category("terminalFr8.Unit")]
    public class Loop_v1Tests
    {
        [Test]
        public void GetDataListSize_ForTableWithHeaders_ReturnsTheActualNumberOfRows()
        {
            var table = new StandardTableDataCM { FirstRowHeaders = true, Table = new List<TableRowDTO> { new TableRowDTO(), new TableRowDTO() } };
            Assert.AreEqual(1, Loop_v1.GetDataListSize(Crate.FromContent(string.Empty, table)), $"{nameof(Loop_v1.GetDataListSize)} should return the count of data rows from table with headers");
        }

        [Test]
        public void GetDataListSize_ForTableWithoutHeaders_ReturnsTableSize()
        {
            var table = new StandardTableDataCM { FirstRowHeaders = false, Table = new List<TableRowDTO> { new TableRowDTO(), new TableRowDTO() } };
            Assert.AreEqual(2, Loop_v1.GetDataListSize(Crate.FromContent(string.Empty, table)), $"{nameof(Loop_v1.GetDataListSize)} should return the count of all rows from table without headers");
        }

        [Test]
        public void GetDataListSize_ForNonTable_ReturnResultOfJsonParsing()
        {
            var rawJson = "[1, 2]";
            Assert.AreEqual(2, Loop_v1.GetDataListSize(Crate.FromJson(string.Empty, JArray.Parse(rawJson))), $"{nameof(Loop_v1.GetDataListSize)} should return the count of all rows from paring crate contents");
        }
    }
}
