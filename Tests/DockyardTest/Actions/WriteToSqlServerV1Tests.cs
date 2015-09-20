using System;
using System.Collections.Generic;
using System.Data;
using AutoMapper;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
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
            PluginAzureSqlServerStructureMapBootstrapper.ConfigureDependencies(PluginAzureSqlServerStructureMapBootstrapper.DependencyType.TEST);
        }

        [Test]
        [Category("Write_To_Sql_Server_v1.Configure")]
        public void WriteToSqlServer_Configure_WithEmptyCrates_ReturnsInitialConfiguration()
        {
            //Arrange 
            ActionDO curAction = FixtureData.TestAction1();

            //create empty crates
            var emptyCrate = FixtureData.CrateStorageWithConnectionString();
            emptyCrate.CrateDTO = new List<CrateDTO>();
            curAction.CrateStorage = JsonConvert.SerializeObject(emptyCrate);

            //Act
            var result = new Write_To_Sql_Server_v1().Configure(Mapper.Map<ActionDTO>(curAction));

            //Assert the connection string value is null or empty
            var connectionStringFieldDefinition = JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(result.CrateDTO[0].Contents);
            Assert.IsTrue(connectionStringFieldDefinition.Count == 1);
            Assert.IsTrue(string.IsNullOrEmpty(connectionStringFieldDefinition[0].Value));
        }

        [Test]
        [Category("Write_To_Sql_Server_v1.Configure")]
        public void WriteToSqlServer_Configure_WithEmptyConnectionStringValue_ReturnsInitialConfiguration()
        {
            //Arrange 
            ActionDO curAction = FixtureData.TestAction1();
            var connectionStringCrate = FixtureData.CrateStorageWithConnectionString();
            curAction.CrateStorage = JsonConvert.SerializeObject(connectionStringCrate);

            //Act
            var result = new Write_To_Sql_Server_v1().Configure(Mapper.Map<ActionDTO>(curAction));

            //Assert the connection string value is null or empty
            var connectionStringFieldDefinition = JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(result.CrateDTO[0].Contents);
            Assert.IsTrue(connectionStringFieldDefinition.Count == 1);
            Assert.IsTrue(string.IsNullOrEmpty(connectionStringFieldDefinition[0].Value));
        }

        [Test]
        [Category("Write_To_Sql_Server_v1.Configure")]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteToSqlServer_Configure_WithTwoConnectionStrings_ShouldThrowArgumentException()
        {
            //Arrange empty crates
            ActionDO curAction = FixtureData.TestAction1();

            //create connection with multiple values
            var connectionStringCrate = FixtureData.CrateStorageWithConnectionString(true, true);
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
            var connectionStringCrate = FixtureData.CrateStorageWithConnectionString();
            IList<FieldDefinitionDTO> connectionStringFields =JsonConvert.DeserializeObject<List<FieldDefinitionDTO>>(connectionStringCrate.CrateDTO[0].Contents);
            connectionStringFields[0].Value =
                @"Data Source=s79ifqsqga.database.windows.net;Initial Catalog=demodb_health;User ID=alexeddodb;Password=Thales89";
            connectionStringCrate.CrateDTO[0].Contents = JsonConvert.SerializeObject(connectionStringFields);
            curAction.CrateStorage = JsonConvert.SerializeObject(connectionStringCrate);

            //Act
            var result = new Write_To_Sql_Server_v1().Configure(Mapper.Map<ActionDTO>(curAction));

            //Assert the connection string value is null or empty

            Assert.IsTrue(result.CrateDTO.Count == 2);
            Assert.AreEqual(result.CrateDTO[0].ManifestType, "Standard Configuration Controls");
            Assert.AreEqual(result.CrateDTO[1].ManifestType, "Standard Design-Time Fields");

            IList<string> dataTableFields =
                JsonConvert.DeserializeObject<List<string>>(result.CrateDTO[1].Contents);
            Assert.AreEqual(dataTableFields.Count, 3);
            Assert.AreEqual(dataTableFields[0], "[Customer].CurrentMedicalCondition");
            Assert.AreEqual(dataTableFields[1], "[Customer].ID");
            Assert.AreEqual(dataTableFields[2], "[Customer].Physician");
        }
    }
}