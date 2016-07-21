using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using NUnit.Framework;
using Fr8.Testing.Integration;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Extract_Data_From_Envelopes_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(1, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count(x => x.Label == "Configuration_Controls"));
        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(2, controls.Controls.Count);
            Assert.IsTrue(controls.Controls[0] is TextArea);
            Assert.AreEqual("TextArea", controls.Controls[0].Type);
            Assert.IsTrue(controls.Controls[1] is DropDownList);
            Assert.AreEqual("FinalActionsList", controls.Controls[1].Name);
        }

        private void AddHubActivityTemplate(Fr8DataDTO dataDTO)
        {
            AddActivityTemplate(dataDTO, HealthMonitor_FixtureData.Monitor_DocuSign_v1_ActivityTemplate_For_Solution());
            AddActivityTemplate(dataDTO, HealthMonitor_FixtureData.Send_DocuSign_Envelope_v1_ActivityTemplate_for_Solution());
        }

        [Test]
        public async Task Extract_Data_From_Envelopes_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestDataDTO = await HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(requestDataDTO);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }

        [Test]
        public async Task Extract_Data_From_Envelopes_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestDataDTO = await HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(requestDataDTO);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Extract_Data_From_Envelopes_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestDataDTO = await HealthMonitor_FixtureData.Extract_Data_From_Envelopes_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(requestDataDTO);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestDataDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
