using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Control;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminalFr8CoreTests.Fixtures;
using Hub.Managers;
using StructureMap.Util;

namespace terminalFr8CoreTests.Integration
{
    [Explicit]
    public class ExecuteSql_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        private void AssertConfigureControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(1, control.Controls.Count);

            // Assert that first control is a TextBlock
            // with Label == "No configuration"
            // with Name == "NoConfigLabel"
            Assert.IsTrue(control.Controls[0] is TextBlock);
            Assert.AreEqual("No configuration", control.Controls[0].Label);
            Assert.AreEqual("NoConfigLabel", control.Controls[0].Name);
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async void ExecuteSql_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = FixtureData.ExecuteSql_InitialConfiguration_ActionDTO();

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());

            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Test run-time for action Run().
        /// </summary>
        [Test]
        public async void ExecuteSql_Run()
        {

            var runUrl = GetTerminalRunUrl();

            var actionDTO = FixtureData.ExecuteSql_InitialConfiguration_ActionDTO();
            
            AddPayloadCrate(
               actionDTO,
               new StandardQueryCM()
               {
                   Queries = new List<QueryDTO>() { new QueryDTO() { Name = "Customer" } }
               }
               ,
               "Sql Query"
            );

            var lstFields = new List<FieldDTO>();
            lstFields.Add(new FieldDTO() { Key = "Customer.Physician", Value = "String" });
            lstFields.Add(new FieldDTO() { Key = "Customer.CurrentMedicalCondition", Value = "String" });
            AddUpstreamCrate(
                actionDTO,
                new StandardDesignTimeFieldsCM(lstFields),
                "Sql Column Types"
            );

            lstFields.Clear();
            lstFields.Add(new FieldDTO() { Key = UtilitiesTesting.Fixtures.FixtureData.TestConnectionString2().Value, Value = "value" });
            AddUpstreamCrate(
                actionDTO,
                new StandardDesignTimeFieldsCM(lstFields),
                "Sql Connection String"
            );

            AddOperationalStateCrate(actionDTO, new OperationalStateCM());

            var responsePayloadDTO =
                await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

            Assert.NotNull(responsePayloadDTO);
            Assert.NotNull(responsePayloadDTO.CrateStorage);
            Assert.NotNull(responsePayloadDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responsePayloadDTO.CrateStorage);
            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardPayloadDataCM>().Count(x => x.Label == "Sql Query Result"));
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardQueryCM>().Count(x => x.Label == "Sql Query"));
        }
    }
}
