using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalDocuSignTests.Fixtures;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Record_DocuSign_Events_v1_Tests : BaseHealthMonitorTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        private void AssertCrateTypes(CrateStorage crateStorage)
        {

            Assert.AreEqual(3, crateStorage.Count);

            Assert.AreEqual(1, crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<EventSubscriptionCM>().Count());
            Assert.AreEqual(1, crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count(x => x.Label == "Available Run-Time Objects"));
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
        public async void Record_DocuSign_Events_Initial_Configuration_Check_Crate_Structure()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Record_Docusign_v1_InitialConfiguration_ActionDTO();

            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            AssertCrateTypes(crateStorage);
            AssertControls(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single());
            AssertList(crateStorage.CrateContentsOfType<EventSubscriptionCM>().Single());
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}",
            MatchType = MessageMatch.Contains
        )]
        public async void Record_DocuSign_Events_Initial_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Record_Docusign_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;

            await HttpPostAsync<ActionDTO, JToken>(
                configureUrl,
                requestActionDTO
            );
        }

        /// <summary>
        /// Test run-time without Auth-Token.
        /// </summary>
        [Test]
        public async void Record_DocuSign_Events_Run_NoAuth()
        {
            var runUrl = GetTerminalRunUrl();
            
            var requestActionDTO = HealthMonitor_FixtureData.Record_Docusign_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;
            AddOperationalStateCrate(requestActionDTO, new OperationalStateCM());
            var payload = await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, requestActionDTO);
            CheckIfPayloadHasNeedsAuthenticationError(payload);
        }

        /// <summary>
        /// Test run-time for action Run().
        /// </summary>
        [Test, Ignore]
        public async void Record_DocuSign_Envelope_Run()
        {
            var runUrl = GetTerminalRunUrl();

            var actionDTO = HealthMonitor_FixtureData.Record_Docusign_v1_InitialConfiguration_ActionDTO();

            var date = DateTime.Now.ToShortDateString();
            var envelopeId = Guid.NewGuid().ToString();
            var accountId = "foo@bar.com";
            var eventId = Guid.NewGuid().ToString();
            var recipientId = Guid.NewGuid().ToString();
            
            AddPayloadCrate(
               actionDTO,
               new EventReportCM()
               {
                   EventPayload = new CrateStorage()
                   {
                        Data.Crates.Crate.FromContent(
                            "EventReport",
                            new StandardPayloadDataCM(
                                new FieldDTO("CompletedDate", date),
                                new FieldDTO("CreateDate", date),
                                new FieldDTO("DeliveredDate", date),
                                new FieldDTO("EnvelopeId", envelopeId),
                                new FieldDTO("Email", accountId),
                                new FieldDTO("SentDate", date),
                                new FieldDTO("Status", "test_status"),
                                new FieldDTO("EventId", eventId),
                                new FieldDTO("Object", "test_object"),
                                new FieldDTO("RecipientId", recipientId)
                            )
                        )
                   },
                   EventNames = "Receive Envelope"
               }
           );

            AddOperationalStateCrate(actionDTO, new OperationalStateCM());

            var responsePayloadDTO =
                await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
            
            var crateStorage = Crate.GetStorage(responsePayloadDTO);
            Assert.AreEqual(1, crateStorage.CrateContentsOfType<DocuSignEnvelopeCM>(x => x.Label == "DocuSign Envelope Manifest").Count());
            Assert.AreEqual(1, crateStorage.CrateContentsOfType<DocuSignEventCM>(x => x.Label == "DocuSign Event Manifest").Count());
        }

        [Test]
        public async void Record_DocuSign_Events_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Record_Docusign_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }

        [Test]
        public async void Record_DocuSign_Events_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.Record_Docusign_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);
            Assert.IsNotNull(Crate.FromDto(responseActionDTO.CrateStorage));
        }
    }
}
