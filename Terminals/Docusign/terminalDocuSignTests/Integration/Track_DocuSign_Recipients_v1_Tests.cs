using System;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using NUnit.Framework;
using Fr8.Testing.Integration;
using terminalDocuSignTests.Fixtures;
using Fr8.Testing.Unit.Fixtures;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Track_DocuSign_Recipients_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {
            Assert.AreEqual(5, crateStorage.Count);
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "AvailableTemplates"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "AvailableHandlers"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "AvailableRecipientEvents"));
            Assert.AreEqual(1, crateStorage.CratesOfType<KeyValueListCM>().Count(x => x.Label == "AvailableRunTimeDataFields"));
            


        }

        private void AssertControls(StandardConfigurationControlsCM controls)
        {
            Assert.AreEqual(5, controls.Controls.Count());

            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "Track_Which_Envelopes"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "NotificationHandler"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "TimePeriod"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "RecipientEvent"));
            Assert.AreEqual(1, controls.Controls.Count(x => x.Name == "EventInfo"));
            
        }

        private void AddHubActivityTemplate(Fr8DataDTO dataDTO)
        {

            var terminal = new TerminalDTO()
            {
                Name = "terminalDocuSign",
                Label = "DocuSign",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                Endpoint = TerminalUrl,
                AuthenticationType = AuthenticationType.Internal
            };
            var terminalCoreDO = FixtureData.TestTerminal_Core_DTO();

            var docusignEventActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Monitor_DocuSign_Envelope_Activity",
                Label = "Monitor DocuSign Envelope Activity",
                Categories = new[] { ActivityCategories.Monitor },
                Terminal = terminal,
                NeedsAuthentication = true,
                MinPaneWidth = 330
            };

            var setDelayActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Set_Delay",
                Label = "Delay Action Processing",
                Categories = new[] { ActivityCategories.Process },
                Terminal = terminalCoreDO,
                NeedsAuthentication = false,
                MinPaneWidth = 330
            };

            var testIncomingDataTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Test_Incoming_Data",
                Label = "Test Incoming Data",
                Categories = new[] { ActivityCategories.Process },
                Terminal = terminalCoreDO,
                NeedsAuthentication = false
            };

            AddActivityTemplate(
               dataDTO,
              testIncomingDataTemplate
            );

            AddActivityTemplate(
               dataDTO,
              setDelayActionTemplate
            );

            var queryFr8WarehouseActionTemplate = new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Query_Fr8_Warehouse",
                Label = "Query Fr8 Warehouse",
                Categories = new[] { ActivityCategories.Process },
                Terminal = terminalCoreDO,
                NeedsAuthentication = false,
                MinPaneWidth = 330
            };

            AddActivityTemplate(
               dataDTO,
              queryFr8WarehouseActionTemplate
            );

            AddActivityTemplate(
               dataDTO,
              docusignEventActionTemplate
            );

            AddActivityTemplate(
                dataDTO,
                new ActivityTemplateDTO()
                {
                    Id = Guid.NewGuid(),
                    Name = "Send Email_Via_Send_Grid",
                    Label = "Send Email",
                    Tags = "Notifier",
                    Version = "1",
                    Terminal = terminal
                }
            );
        }

        [Test]
        public async Task Track_DocuSign_Recipients_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);
            AddHubActivityTemplate(dataDTO);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    dataDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
        }
        [Test]
        public async Task Track_DocuSign_Recipients_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async Task Track_DocuSign_Recipients_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Track_DocuSign_Recipients_v1_InitialConfiguration_Fr8DataDTO(this);

            //Act
            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
