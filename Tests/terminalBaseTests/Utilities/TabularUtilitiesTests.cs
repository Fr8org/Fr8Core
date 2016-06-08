using NUnit.Framework;
using System;
using Fr8.Infrastructure.Data.Manifests;
using terminalUtilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace terminaBaselTests.Utilities
{
    [TestFixture]
    [Category("BaseTerminalEvent")]
    public class TabularUtilitiesTests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void OneRowTable_ShouldReturnCrate_When_OneRow_FirstRowHeaders_ConfigTime()
        {
            var crate = TabularUtilities.PrepareFieldsForOneRowTable(true, false, FixtureData.TestStandardTableData().Table);

            Assert.NotNull(crate, "TabularUtilities#PrepareFieldsForOneRowTable should not return null with provided with one-row table");
            Assert.IsInstanceOf<FieldDescriptionsCM>(crate.Get());
            Assert.AreEqual(6, crate.Get<FieldDescriptionsCM>().Fields.Count);
        }

        [Test]
        public void OneRowTable_ShouldReturnCrate_When_OneRow_FirstRowHeaders_RunTime()
        {
            var crate = TabularUtilities.PrepareFieldsForOneRowTable(true, true, FixtureData.TestStandardTableData().Table);

            Assert.NotNull(crate, "TabularUtilities#PrepareFieldsForOneRowTable should not return null with provided with one-row table");
            Assert.IsInstanceOf<StandardPayloadDataCM>(crate.Get());
            Assert.AreEqual(6, crate.Get<StandardPayloadDataCM>().PayloadObjects[0].PayloadObject.Count);
        }

        [Test]
        public void OneRowTable_ShouldReturnCrate_When_OneRow_RunTime()
        {
            var crate = TabularUtilities.PrepareFieldsForOneRowTable(false, true, FixtureData.TestStandardTableData_NoHeader().Table, FixtureData.TestStandardTableData_HeadersOnly());

            Assert.NotNull(crate, "TabularUtilities#PrepareFieldsForOneRowTable should not return null with provided with one-row table");
            Assert.IsInstanceOf<StandardPayloadDataCM>(crate.Get());
            Assert.AreEqual(6, crate.Get<StandardPayloadDataCM>().PayloadObjects[0].PayloadObject.Count);
        }

        [Test]
        public void OneRowTable_ShouldReturnCrate_When_OneRow_ConfigTime()
        {
            var crate = TabularUtilities.PrepareFieldsForOneRowTable(false, false, FixtureData.TestStandardTableData_NoHeader().Table, FixtureData.TestStandardTableData_HeadersOnly());

            Assert.NotNull(crate, "TabularUtilities#PrepareFieldsForOneRowTable should not return null with provided with one-row table");
            Assert.IsInstanceOf<FieldDescriptionsCM>(crate.Get());
            Assert.AreEqual(6, crate.Get<FieldDescriptionsCM>().Fields.Count);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void OneRowTable_ShouldThrow_When_HeadersNotProvided()
        {
            var crate = TabularUtilities.PrepareFieldsForOneRowTable(false, false, FixtureData.TestStandardTableData_NoHeader().Table, null);
        }

        [Test]
        public void OneRowTable_ShouldReturnNull_When_MultipleRows()
        {
            var crate = TabularUtilities.PrepareFieldsForOneRowTable(false, false, FixtureData.TestStandardTableData_TwoRowsNoHeader().Table, FixtureData.TestStandardTableData_HeadersOnly());
            Assert.Null(crate, "TabularUtilities#PrepareFieldsForOneRowTable should return null with provided with a table containing multiple rows");
        }

        [Test]
        public void OneRowTable_ShouldReturnNull_When_FirstRowHeaders_MultipleRows()
        {
            var crate = TabularUtilities.PrepareFieldsForOneRowTable(true, false, FixtureData.TestStandardTableData_TwoRows().Table);
            Assert.Null(crate, "TabularUtilities#PrepareFieldsForOneRowTable should return null with provided with a table containing multiple rows");
        }
    }
}
