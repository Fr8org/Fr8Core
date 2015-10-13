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
using pluginExcel.Infrastructure;
using Moq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Data.Repositories;

namespace pluginTests.PluginExcelTests
{
    [TestFixture]
    [Category("pluginExcel")]
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
        public void ConfigEvaluatorInitialResponse_Test()
        {
            var curActionDTO = new ActionDTO()
            {
                CrateStorage = new CrateStorageDTO()
                {
                    CrateDTO = new System.Collections.Generic.List<CrateDTO>(),
                },
            };

            var result = new ExtractData_v1().ConfigurationEvaluator(curActionDTO);

            Assert.AreEqual(result, PluginBase.Infrastructure.ConfigurationRequestType.Initial);
        }

        //[Test]
        //[ExpectedException]
        //public void ConfigEvaluatorFollowupResponseThrowsException_Test()
        //{
        //    var curActionDTO = new ActionDTO()
        //    {
        //        CrateStorage = new CrateStorageDTO()
        //        {
        //            CrateDTO = new System.Collections.Generic.List<CrateDTO>(),
        //        },
        //    };
        //    StandardConfigurationControlsMS confControlsMS = new StandardConfigurationControlsMS()
        //    {
        //        Controls = new List<ControlDefinitionDTO>()
        //        {
        //            new ControlDefinitionDTO("select_file", true, "1", "select file"),
        //            new ControlDefinitionDTO("select_file", true, "2", "select file"),
        //        },
        //    };
        //    curActionDTO.CrateStorage.CrateDTO.Add(new CrateDTO()
        //    {
        //        Contents = JsonConvert.SerializeObject(confControlsMS),
        //        ManifestType = CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
        //    });
        //    //Mock<ICrate> crateMock = new Mock<ICrate>();
        //    //crateMock.Setup(a => a.GetElementByKey<int>(It.IsAny<IEnumerable<CrateDTO>>(), It.IsAny<int>(), It.IsAny<string>())).Returns(() => new List<JObject>() { new JObject(), new JObject() });

        //    //ActionDO actionDO = new FixtureData(uow).TestAction3();
        //    //var controller = new ActionController(crateMock.Object);


        //    var result = new ExtractData_v1().ConfigurationEvaluator(curActionDTO);

        //    Assert.AreNotEqual(result, PluginBase.Infrastructure.ConfigurationRequestType.Followup);
        //}

        //[Test]
        //public void ConfigEvaluatorFollowupResponse_Test()
        //{
        //    var curActionDTO = new ActionDTO()
        //    {
        //        CrateStorage = new CrateStorageDTO()
        //        {
        //            CrateDTO = new System.Collections.Generic.List<CrateDTO>(),
        //        },
        //    };
        //    StandardConfigurationControlsMS confControlsMS = new StandardConfigurationControlsMS()
        //    {
        //        Controls = new List<ControlDefinitionDTO>()
        //        {
        //            new ControlDefinitionDTO("select_file", true, "1", "select file"),
        //        },
        //    };
        //    curActionDTO.CrateStorage.CrateDTO.Add(new CrateDTO()
        //        {
        //            Contents = JsonConvert.SerializeObject(confControlsMS),
        //            ManifestType = CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
        //        });
        //    //Mock<ICrate> crateMock = new Mock<ICrate>();
        //    //crateMock.Setup(a => a.GetElementByKey<int>(It.IsAny<IEnumerable<CrateDTO>>(), It.IsAny<int>(), It.IsAny<string>())).Returns(() => new List<JObject>() { new JObject(), new JObject() });

        //    //ActionDO actionDO = new FixtureData(uow).TestAction3();
        //    //var controller = new ActionController(crateMock.Object);


        //    var result = new ExtractData_v1().ConfigurationEvaluator(curActionDTO);

        //    Assert.AreEqual(result, PluginBase.Infrastructure.ConfigurationRequestType.Followup);
        //}

        [Test]
        public async void CallExtractData_Execute()
        {
            var bytesFromExcel = PluginFixtureData.TestExcelData();
            var columnHeaders = ExcelUtils.GetColumnHeaders(bytesFromExcel, "xlsx");
            var excelRows = ExcelUtils.GetTabularData(bytesFromExcel, "xlsx");
            var tableDataMS = new StandardTableDataCM()
            {
                FirstRowHeaders = true,
                Table = ExcelUtils.CreateTableCellPayloadObjects(excelRows, columnHeaders),
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
            var payloadDataMS = JsonConvert.DeserializeObject<StandardPayloadDataCM>(payloadCrates.First().Contents);

            Assert.IsNotNull(result.CrateStorage);
            Assert.IsNotNull(payloadCrates);
            Assert.IsNotNull(payloadDataMS);
            Assert.AreEqual(payloadDataMS.PayloadObjects.Count, 3);
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Key, "FirstName");
            Assert.AreEqual(payloadDataMS.PayloadObjects[0].PayloadObject[0].Value, "Alex");
        }
    }
}
