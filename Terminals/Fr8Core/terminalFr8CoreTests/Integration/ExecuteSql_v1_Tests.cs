using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalFr8CoreTests.Fixtures;

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
        public async Task ExecuteSql_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = FixtureData.ExecuteSql_InitialConfiguration_Fr8DataDTO();

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());

            AssertConfigureControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        /// <summary>
        /// Test run-time for action Run().
        /// </summary>
        [Test]
        public async Task ExecuteSql_Run()
        {
            var runUrl = GetTerminalRunUrl();

            var dataDTO = FixtureData.ExecuteSql_InitialConfiguration_Fr8DataDTO();
            
            AddPayloadCrate(
               dataDTO,
               new StandardQueryCM()
               {
                   Queries = new List<QueryDTO>() { new QueryDTO() { Name = "Customer" } }
               }
               ,
               "Sql Query"
            );

            var lstFields = new List<KeyValueDTO>();
            lstFields.Add(new KeyValueDTO() { Key = "Customer.Physician", Value = "String" });
            lstFields.Add(new KeyValueDTO() { Key = "Customer.CurrentMedicalCondition", Value = "String" });
            AddUpstreamCrate(
                dataDTO,
                new KeyValueListCM(lstFields),
                "Sql Column Types"
            );

            lstFields.Clear();
            lstFields.Add(new KeyValueDTO() { Key = Fr8.Testing.Unit.Fixtures.FixtureData.TestConnectionString2().Value, Value = "value" });
            AddUpstreamCrate(
                dataDTO,
                new KeyValueListCM(lstFields),
                "Sql Connection String"
            );

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var responsePayloadDTO =
                await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

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
