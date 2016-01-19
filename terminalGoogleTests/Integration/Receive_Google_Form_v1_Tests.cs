using System;
using NUnit.Framework;
using HealthMonitor.Utility;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.Control;
using Data.Crates;
using System.Linq;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Newtonsoft.Json.Linq;

namespace terminalGoogleTests.Unit
{
    [Explicit]
    public class Receive_Google_Form_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalGoogle"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Receive_Google_Form_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Receive_Google_Form_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            Assert.AreEqual(3, crateStorage.Count);
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault());
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardDesignTimeFieldsCM>().SingleOrDefault());
            Assert.IsNotNull(crateStorage.CrateContentsOfType<EventSubscriptionCM>().SingleOrDefault());
        }

        /// <summary>
        /// Validate correct crate-storage CM structure.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Receive_Google_Form_Initial_Configuration_Check_CM_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Receive_Google_Form_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            var standardConfigurationControlsCM = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
            var standardDesignTimeFieldsCM = crateStorage.CrateContentsOfType<StandardDesignTimeFieldsCM>().SingleOrDefault();
            var eventSubscriptionCM = crateStorage.CrateContentsOfType<EventSubscriptionCM>().SingleOrDefault();

            var dropdown = standardConfigurationControlsCM.Controls.Where(s => s.GetType() == typeof(DropDownList)).FirstOrDefault();

            Assert.IsNotNull(dropdown);
            Assert.AreEqual("Selected_Google_Form", dropdown.Name);
            Assert.AreEqual("Available Forms", dropdown.Source.Label);
            Assert.AreEqual(CrateManifestTypes.StandardDesignTimeFields, dropdown.Source.ManifestType);

            Assert.IsNotNull(standardDesignTimeFieldsCM);
            Assert.AreEqual(1, crateStorage.Where(s => s.Label == "Available Forms").Count());

            Assert.IsNotNull(eventSubscriptionCM);
            Assert.AreEqual(1, crateStorage.Where(s => s.Label == "Standard Event Subscriptions").Count());
        }

        /// <summary>
        /// Validate dropdownlist source contains google forms(pre-installed in users google drive)
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Receive_Google_Form_Initial_Configuration_Check_Source_Fields()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Receive_Google_Form_v1_InitialConfiguration_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var standardDesignTimeFieldsCM = crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Where(x => x.Label == "Available Forms").ToArray();

            Assert.IsNotNull(standardDesignTimeFieldsCM);
            Assert.Greater(standardDesignTimeFieldsCM.Count(), 0);
            Assert.Greater(standardDesignTimeFieldsCM.First().Content.Fields.Count(), 0);
        }

        /// <summary>
        /// Wait for HTTP-500 exception when Auth-Token is not passed to initial configuration.
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""One or more errors occurred.""}"
        )]
        public async void Receive_Google_Form_Initial_Configuration_NoAuth()
        {
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.Receive_Google_Form_v1_InitialConfiguration_ActionDTO();
            requestActionDTO.AuthToken = null;

            await HttpPostAsync<ActionDTO, JToken>(
                configureUrl,
                requestActionDTO
            );
        }

        /// <summary>
        /// Validate google app script is uploaded in users google drive
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Receive_Google_Form_Activate_Check_Script_Exist()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Receive_Google_Form_v1_ActivateDeactivate_ActionDTO();

            //Act
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.IsNotNull(responseActionDTO);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);
            var formID = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().SingleOrDefault();

            Assert.Greater(formID.PayloadObjects.SelectMany(s => s.PayloadObject).Count(), 0);
        }

        [Test, Category("Integration.terminalGoogle")]
        public async void Receive_Google_Form_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Receive_Google_Form_v1_ActivateDeactivate_ActionDTO();

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

        [Test, Category("Integration.terminalGoogle")]
        public async void Receive_Google_Form_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = fixture.Receive_Google_Form_v1_ActivateDeactivate_ActionDTO();

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

        /// <summary>
        /// Should throw exception if cannot extract any data from google form
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"{""status"":""terminal_error"",""message"":""EventReportCrate is empty.""}"
            )]
        public async void Receive_Google_Form_Run_WithInvalidPapertrailUrl_ShouldThrowException()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            //prepare the action DTO with valid target URL
            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var actionDTO = fixture.Receive_Google_Form_v1_Run_EmptyPayload();

            //Act
            await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);
        }

        /// <summary>
        /// Should return more than one payload fielddto for the response
        /// </summary>
        [Test, Category("Integration.terminalGoogle")]
        public async void Receive_Google_Form_Run_Returns_Payload()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var actionDTO = fixture.Receive_Google_Form_v1_Run_ActionDTO();

            AddPayloadCrate(
               actionDTO,
               new EventReportCM()
               {
                   EventPayload = new CrateStorage()
                   {
                        Data.Crates.Crate.FromContent(
                            "Response",
                            new StandardPayloadDataCM(
                                new FieldDTO("response", "key1=value1&key2=value2")
                            )
                        )
                   }
               }
            );

            //Act
            var responsePayloadDTO =
                await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

            //Assert
            var crateStorage = Crate.FromDto(responsePayloadDTO.CrateStorage);

            var standardDesignTimeFieldsCM = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().SingleOrDefault();

            Assert.IsNotNull(standardDesignTimeFieldsCM);
            var fields = standardDesignTimeFieldsCM.PayloadObjects.SelectMany(s => s.PayloadObject);
            Assert.Greater(fields.Count(), 0);
        }
    }
}
