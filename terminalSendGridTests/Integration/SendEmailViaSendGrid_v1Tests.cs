using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalSendGridTests.Fixtures;

namespace terminalSendGridTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    public class SendEmailViaSendGrid_v1Tests : BaseHealthMonitorTest
    {
        ActionDTO actionDTOInit = new ActionDTO();

        public override string TerminalName
        {
            get { return "terminalSendGrid"; }
        }

        /// <summary>
        /// Validate correct crate-storage structure in initial configuration response.
        /// </summary>
        [Test, Category("Integration.terminalSendGrid")]
        public async void SendEmailViaSendGrid_Initial_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_ActionDTO();

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
            actionDTOInit = responseActionDTO;
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault());
            Assert.IsNotNull(crateStorage.CrateContentsOfType<StandardDesignTimeFieldsCM>().SingleOrDefault());

            var standardConfigurationControlCM = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>();
            Assert.AreEqual(1, standardConfigurationControlCM.Where(w => w.FindByName("EmailAddress") != null).Count());
            Assert.AreEqual(1, standardConfigurationControlCM.Where(w => w.FindByName("EmailSubject") != null).Count());
            Assert.AreEqual(1, standardConfigurationControlCM.Where(w => w.FindByName("EmailBody") != null).Count());
        }

        /// <summary>
        /// Validate correct crate-storage structure in followup configuration response.
        /// </summary>
        [Test, Category("Integration.terminalSendGrid")]
        public async void SendEmailViaSendGrid_FollowUp_Configuration_Check_Crate_Structure()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var requestActionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_ActionDTO();

            //Act
            //Call first time for the initial configuration
            var responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Call second time for the follow up configuration
            responseActionDTO =
                await HttpPostAsync<ActionDTO, ActionDTO>(
                    configureUrl,
                    requestActionDTO
                );

            //Assert
            Assert.NotNull(responseActionDTO);
            Assert.NotNull(responseActionDTO.CrateStorage);
            Assert.NotNull(responseActionDTO.CrateStorage.Crates);

            var crateStorage = Crate.FromDto(responseActionDTO.CrateStorage);

            Assert.AreEqual(1, crateStorage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x => x.Label == "Upstream Terminal-Provided Fields").Count());
        }

        [Test, Category("Integration.terminalSendGrid")]
        public async void SendEmailViaSendGrid_Run_Returns_Payload()
        {
            //Arrange
            var runUrl = GetTerminalRunUrl();

            var actionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_ActionDTO();


            using (var updater = Crate.UpdateStorage(actionDTO))
            {
                updater.CrateStorage.Add(CreateCrates());
            }

            AddOperationalStateCrate(actionDTO, new OperationalStateCM());

            AddPayloadCrate(
               actionDTO,
               new StandardPayloadDataCM() { }
            );

            //Act
            var responsePayloadDTO =
                await HttpPostAsync<ActionDTO, PayloadDTO>(runUrl, actionDTO);

            //Assert
            var crateStorage = Crate.FromDto(responsePayloadDTO.CrateStorage);

            var StandardPayloadDataCM = crateStorage.CrateContentsOfType<StandardPayloadDataCM>().SingleOrDefault();

            Assert.IsNotNull(StandardPayloadDataCM);
        }

        private Crate CreateCrates()
        {
            var control = new TextSource()
            {
                Name = "EmailAddress",
                ValueSource = "specific",
                Value = "test@mail.com"
            };

            var control2 = new TextSource()
            {
                Name = "EmailSubject",
                ValueSource = "specific",
                Value = "test subject"
            };

            var control3 = new TextSource()
            {
                Name = "EmailBody",
                ValueSource = "specific",
                Value = "test body"
            };

            return PackControlsCrate(control, control2, control3);
        }

        private Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            var controls = new StandardConfigurationControlsCM(controlsList);
            return Crate<StandardConfigurationControlsCM>.FromContent("Configuration_Controls", controls);
        }

        [Test, Category("Integration.terminalSendGrid")]
        public async void SendEmailViaSendGrid_Activate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalActivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_ActionDTO();

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

        [Test, Category("Integration.terminalSendGrid")]
        public async void SendEmailViaSendGrid_Deactivate_Returns_ActionDTO()
        {
            //Arrange
            var configureUrl = GetTerminalDeactivateUrl();

            HealthMonitor_FixtureData fixture = new HealthMonitor_FixtureData();
            var requestActionDTO = HealthMonitor_FixtureData.SendEmailViaSendGrid_v1_InitialConfiguration_ActionDTO();

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
