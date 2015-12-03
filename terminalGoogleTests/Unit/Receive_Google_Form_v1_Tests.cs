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
    public class Receive_Google_Form_v1_Tests : BaseHealthMonitorTest
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

            var standardDesignTimeFieldsCM = crateStorage.CrateContentsOfType<StandardDesignTimeFieldsCM>().SingleOrDefault();
            
            Assert.IsNotNull(standardDesignTimeFieldsCM);
            Assert.AreEqual(1, crateStorage.Where(s => s.Label == "Available Forms").Count());

            Assert.Greater(0, standardDesignTimeFieldsCM.Fields.Count());
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
    }
}
