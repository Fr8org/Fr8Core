using Fr8.Testing.Integration;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Prepare_DocuSign_Events_For_Storage_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(ICrateStorage crateStorage)
        {

            Assert.AreEqual(2, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count());
        }

        private void AssertControls(StandardConfigurationControlsCM control)
        {
            Assert.AreEqual(1, control.Controls.Count);

            // Assert that first control is a TextBlock 
            // with Label == "Monitor All DocuSign events"
            // with Value == "This Action doesn't require any configuration."
            Assert.IsTrue(control.Controls[0] is TextBlock);
            Assert.AreEqual("Monitor All DocuSign events", control.Controls[0].Label);
            Assert.AreEqual("This Action doesn't require any configuration.", control.Controls[0].Value);
        }

        private void AssertList(EventSubscriptionCM control)
        {
            Assert.IsNotNull(control.Subscriptions);
            Assert.IsTrue(control.Subscriptions.Count > 0);
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test]
        public async Task Prepare_DocuSign_Events_For_Storage_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = await HealthMonitor_FixtureData.Prepare_DocuSign_Events_For_Storage_v1_InitialConfiguration_Fr8DataDTO(this);

            var responseActionDTO =
                await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
            AssertList(crateStorage.CrateContentsOfType<EventSubscriptionCM>().Single());
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test]
        public async Task Prepare_DocuSign_Events_For_Storage_Initial_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var dataDTO = await HealthMonitor_FixtureData.Prepare_DocuSign_Events_For_Storage_v1_InitialConfiguration_Fr8DataDTO(this);
            dataDTO.ActivityDTO.AuthToken = null;

            var response = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(
                configureUrl,
                dataDTO
            );

            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test]
        public async Task Prepare_DocuSign_Events_For_Storage_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();

            var dataDTO = await HealthMonitor_FixtureData.Prepare_DocuSign_Events_For_Storage_v1_InitialConfiguration_Fr8DataDTO(this);
            dataDTO.ActivityDTO.AuthToken = null;
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            var payload = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }

        /// <summary>
        /// Test run-time for action Run().
        /// </summary>
        [Test]
        public async Task Prepare_DocuSign_Events_For_Storage_Envelope_Run()
        {
            var runUrl = GetTerminalRunUrl();

            var dataDTO = await HealthMonitor_FixtureData.Prepare_DocuSign_Events_For_Storage_v1_InitialConfiguration_Fr8DataDTO(this);

            var date = DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo.ShortDatePattern);
            var envelopeId = Guid.NewGuid().ToString();
            var accountId = "foo@bar.com";
            var eventId = Guid.NewGuid().ToString();
            var recipientId = Guid.NewGuid().ToString();

            var envelopePayload = HealthMonitor_FixtureData.GetEnvelopePayload();


            AddPayloadCrate(
               dataDTO,
               new EventReportCM()
               {
                   EventPayload = envelopePayload,
                   EventNames = "Receive Envelope"
               }
           );

            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            var responsePayloadDTO =
                await HttpPostAsync<Fr8DataDTO, PayloadDTO>(runUrl, dataDTO);

            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            Assert.AreEqual(1, crateStorage.CrateContentsOfType<DocuSignEnvelopeCM_v2>(x => x.Label == "DocuSign Envelope").Count());
        }

        [Test]
        public async Task Prepare_DocuSign_Events_For_Storage_Activate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Prepare_DocuSign_Events_For_Storage_v1_InitialConfiguration_Fr8DataDTO(this);

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
        public async Task Prepare_DocuSign_Events_For_Storage_Deactivate_Returns_ActivityDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = await HealthMonitor_FixtureData.Prepare_DocuSign_Events_For_Storage_v1_InitialConfiguration_Fr8DataDTO(this);

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
