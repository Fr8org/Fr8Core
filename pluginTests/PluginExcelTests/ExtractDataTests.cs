using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces;
using NUnit.Framework;
using pluginAzureSqlServer;
using StructureMap;
using Utilities;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using pluginTests.Fixtures;
using pluginExcel.Actions;
using Core.Interfaces;
using Newtonsoft.Json;

namespace pluginTests.PluginExcelTests
{
    [TestFixture]
    public class ExtractDataTests : BaseTest
    {
        public const string ExcelTestServerUrl = "ExcelTestServerUrl";

        public const string filesCommand = "files";

        private IAction _action;
        private ICrate _crate;
        private FixtureData _fixtureData;
        private IDisposable _server;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _fixtureData = new FixtureData(ObjectFactory.GetInstance<IUnitOfWork>());
            _action = ObjectFactory.GetInstance<IAction>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        [TearDown]
        public void Cleanup()
        {
            
        }

        [Test]
        public async void CallExtractData_Execute()
        {
            var bytesFromExcel = PluginFixtureData.TestExcelData();
            var columnHeaders = PluginFixtureData.TestColumnHeaders();
            var excelRows = PluginFixtureData.TestRows();
            var tableDataMS = new StandardTableDataMS()
            {
                FirstRowHeaders = true,
                Table = new ExtractData_v1().ConvertRowsDictToListOfTableRowDTO(excelRows, columnHeaders),
            };

            var curActionDTO = new ActionDTO()
            {
                CrateStorage = new CrateStorageDTO()
                {
                    CrateDTO = new System.Collections.Generic.List<CrateDTO>() 
                    { 
                        new CrateDTO()
                        {
                            ManifestId = CrateManifests.STANDARD_TABLE_DATA_MANIFEST_ID,
                            ManifestType = CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME,
                            Contents = JsonConvert.SerializeObject(tableDataMS),
                        },
                    },
                },
            };

            var result = await new ExtractData_v1().Execute(curActionDTO);
            var payloadCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME, result.CrateStorage);
            var payloadDataMS = JsonConvert.DeserializeObject<StandardPayloadDataMS>(payloadCrates.First().Contents);

            Assert.IsNotNull(result.CrateStorage);
            Assert.IsNotNull(payloadCrates);
            Assert.IsNotNull(payloadDataMS);
            Assert.AreEqual(payloadDataMS.PayloadObjects.Count, 3);
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Key, "FirstName");
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Value, "Alex");
            
        }
    }
}
