using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using pluginAzureSqlServer;
using pluginAzureSqlServer.Actions;
using pluginAzureSqlServer.Infrastructure;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Actions
{
    [TestFixture]
    public class WriteToSqlServerV1Tests : BaseTest
    {
        private const string PayloadData =
            "{'payload': {'Physician' : 'Johnson','CurrentMedicalCondition' : 'Marthambles'}}";

        private Write_To_Sql_Server_v1 _sqServerWriter;
        private string _connectionString;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            ObjectFactory.Configure(PluginAzureSqlServerStructureMapRegistries.TestConfiguration);
        }

        [Test]
        [Category("Write_To_Sql_Server_v1.Configure")]
        public void WriteToSqlServer_Configure_WithEmptyCrates_ReturnsInitialConfiguration()
        {
            //Arrange 
            ActionDO curAction = FixtureData.TestAction1();

            //create empty crates
            var emptyCrate = FixtureData.TestCrateStorage();
            emptyCrate.CrateDTO = new List<CrateDTO>();
            curAction.CrateStorage = JsonConvert.SerializeObject(emptyCrate);

            //Act
            ActionDTO currentActionDTO = new Write_To_Sql_Server_v1().Configure(Mapper.Map<ActionDTO>(curAction));

            //Assert the connection string value is null or empty
            var connectionStringConfigControls = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(currentActionDTO.CrateStorage.CrateDTO[0].Contents);
            Assert.IsTrue(connectionStringConfigControls.Controls.Count == 1);
            Assert.IsTrue(string.IsNullOrEmpty(connectionStringConfigControls.Controls[0].Value));
        }

        [Test]
        [Category("Write_To_Sql_Server_v1.Configure")]
        public void WriteToSqlServer_Configure_WithEmptyConnectionStringValue_ReturnsInitialConfiguration()
        {
            //Arrange 
            ActionDO curAction = FixtureData.TestAction1();
            var connectionStringCrate = FixtureData.TestCrateStorage();
            curAction.CrateStorage = JsonConvert.SerializeObject(connectionStringCrate);

            //Act
            ActionDTO actionDTO = new Write_To_Sql_Server_v1().Configure(Mapper.Map<ActionDTO>(curAction));

            //Assert the connection string value is null or empty
            var connectionStringFieldDefinition = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(actionDTO.CrateStorage.CrateDTO[0].Contents);
            Assert.IsTrue(connectionStringFieldDefinition.Controls.Count == 1);
            Assert.IsTrue(string.IsNullOrEmpty(connectionStringFieldDefinition.Controls[0].Value));
        }

        [Test]
        [Category("Write_To_Sql_Server_v1.Configure")]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteToSqlServer_Configure_WithTwoConnectionStrings_ShouldThrowArgumentException()
        {
            //Arrange empty crates
            ActionDO curAction = FixtureData.TestAction1();

            //Create two connection string crates
            var connectionStringCrate = FixtureData.TestCrateStorage();
            var connectionStringFields = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(connectionStringCrate.CrateDTO[0].Contents);
            connectionStringFields.Controls.Add(FixtureData.TestConnectionStringFieldDefinition());

            //set the values for the connection strings
            connectionStringFields.Controls.ForEach(field => field.Value = "somevalue");

            connectionStringCrate.CrateDTO[0].Contents = JsonConvert.SerializeObject(connectionStringFields);
            curAction.CrateStorage = JsonConvert.SerializeObject(connectionStringCrate);

            //Act
            var result = new Write_To_Sql_Server_v1().Configure(Mapper.Map<ActionDTO>(curAction));
        }

        [Test]
        [Category("Write_To_Sql_Server_v1.Configure")]
        public void WriteToSqlServer_Configure_WithOneConnectionStringValue_ReturnsFollowupConfiguration()
        {
            //Arrange
            ActionDO curAction = FixtureData.TestAction1();

            //create connection string value crates with a vald connection string
            var connectionStringCrate = FixtureData.TestCrateStorage();
            var connectionStringFields = JsonConvert.DeserializeObject<StandardConfigurationControlsMS>(connectionStringCrate.CrateDTO[0].Contents);
            connectionStringFields.Controls[0].Value =
                @"Data Source=s79ifqsqga.database.windows.net;Initial Catalog=demodb_health;User ID=alexeddodb;Password=Thales89";
            connectionStringCrate.CrateDTO[0].Contents = JsonConvert.SerializeObject(connectionStringFields);
            curAction.CrateStorage = JsonConvert.SerializeObject(connectionStringCrate);

            //Act
            ActionDTO actionDTO = new Write_To_Sql_Server_v1().Configure(Mapper.Map<ActionDTO>(curAction));

            //Assert the connection string value is null or empty

            Assert.IsTrue(actionDTO.CrateStorage.CrateDTO.Count == 2);
            Assert.AreEqual(actionDTO.CrateStorage.CrateDTO[0].ManifestType, CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);
            Assert.AreEqual(actionDTO.CrateStorage.CrateDTO[1].ManifestType, CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);

            StandardDesignTimeFieldsMS dataTableFields =
                JsonConvert.DeserializeObject<StandardDesignTimeFieldsMS>(actionDTO.CrateStorage.CrateDTO[1].Contents);
            Assert.AreEqual(dataTableFields.Fields.Count, 3);
            Assert.AreEqual(dataTableFields.Fields[0].Value, "[Customer].CurrentMedicalCondition");
            Assert.AreEqual(dataTableFields.Fields[1].Value, "[Customer].ID");
            Assert.AreEqual(dataTableFields.Fields[2].Value, "[Customer].Physician");
        }
    }
}