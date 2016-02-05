
using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using NUnit.Framework;
using terminalSalesforceTests.Fixtures;
using Data.Control;
using Data.Interfaces.DataTransferObjects.Helpers;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    public class Create_Account_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async void Create_Account_Initial_Configuration_Check_Crate_Structure()
        {
            //Act
            var initialConfigActionDto = await PerformInitialConfiguration();

            //Assert
            Assert.IsNotNull(initialConfigActionDto.CrateStorage,
                "Initial Configuration of Create Account activity contains no crate storage");

            AssertConfigurationControls(Crate.FromDto(initialConfigActionDto.CrateStorage));
        }

        [Test, Category("intergration.terminalSalesforce")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException)
        )]
        public async void Create_Account_Initial_Configuration_Without_AuthToken_Exception_Thrown()
        {
            //Arrange
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create account action DTO
            var dataDTO = HealthMonitor_FixtureData.Create_Account_v1_InitialConfiguration_Fr8DataDTO();
            dataDTO.ActivityDTO.AuthToken = null;

            //Act
            //perform post request to terminal and return the result
            await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, dataDTO);
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async void Create_Account_Run_With_NoAuth_Check_NoAuthProvided_Error()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            initialConfigActionDto = SetAccountName(initialConfigActionDto);
            AddOperationalStateCrate(initialConfigActionDto, new OperationalStateCM());
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);

            //Assert
            Assert.IsNotNull(responseOperationalState);
            var curOperationalState =
                Crate.FromDto(responseOperationalState.CrateStorage).CratesOfType<OperationalStateCM>().Single().Content;
            ErrorDTO errorMessage;
            curOperationalState.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);

            Assert.AreEqual("No AuthToken provided.", errorMessage.Message, "Authentication is mishandled at activity side."); 
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async void Create_Account_Run_With_NoAccountName_Check_NoAccountNameProvided_Error()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            initialConfigActionDto.AuthToken = HealthMonitor_FixtureData.Salesforce_AuthToken();
            AddOperationalStateCrate(initialConfigActionDto, new OperationalStateCM());
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);

            //Assert
            Assert.IsNotNull(responseOperationalState);
            var curOperationalState =
                Crate.FromDto(responseOperationalState.CrateStorage).CratesOfType<OperationalStateCM>().Single().Content;
            ErrorDTO errorMessage;
            curOperationalState.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);
            Assert.AreEqual("No account name found in activity.", errorMessage.Message, "Action works without account name");
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async void Create_Account_Run_With_ValidParameter_Check_PayloadDto_OperationalState()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            initialConfigActionDto = SetAccountName(initialConfigActionDto);
            initialConfigActionDto.AuthToken = HealthMonitor_FixtureData.Salesforce_AuthToken();
            AddOperationalStateCrate(initialConfigActionDto, new OperationalStateCM());
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);

            //Assert
            Assert.IsNotNull(responseOperationalState);
       }

        /// <summary>
        /// Performs Initial Configuration request of Create Account action
        /// </summary>
        private async Task<ActivityDTO> PerformInitialConfiguration()
        {
            //get the terminal configure URL
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create account action DTO
            var requestActionDTO = HealthMonitor_FixtureData.Create_Account_v1_InitialConfiguration_Fr8DataDTO();

            //perform post request to terminal and return the result
            var resultActionDto = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, requestActionDTO);

            using (var updater = Crate.UpdateStorage(resultActionDto))
            {
                var controls = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
                controls.Controls.OfType<TextSource>().ToList().ForEach(ctl => ctl.ValueSource = "specific");
            }

            return resultActionDto;
        }

        private void AssertConfigurationControls(CrateStorage curActionCrateStorage)
        {
            var configurationControls = curActionCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();

            Assert.AreEqual(16, configurationControls.Content.Controls.Count,
                "Create Account does not contain the required 16 fields.");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("Name")),
                "Create Account activity does not have Account Name control");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("AccountNumber")),
                "Create Account does not have Account Number control");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("Phone")),
                "Create Account does not have Phone control");

            //@AlexAvrutin: Commented this since these textboxes do not require requestConfig event. 
            //Assert.IsFalse(configurationControls.Content.Controls.Any(ctrl => ctrl.Events.Count != 1),
            //    "Create Account controls are not subscribed to on Change events");
        }

        private ActivityDTO SetAccountName(ActivityDTO curActivityDto)
        {
            using (var updater = Crate.UpdateStorage(curActivityDto))
            {
                var controls = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var targetUrlTextBox = (TextSource)controls.Controls[0];
                targetUrlTextBox.ValueSource = "specific";
                targetUrlTextBox.TextValue = "IntegrationTestAccount";
            }

            return curActivityDto;
        }
    }
}
