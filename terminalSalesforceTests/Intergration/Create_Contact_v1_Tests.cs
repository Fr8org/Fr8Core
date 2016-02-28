
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using NUnit.Framework;
using terminalSalesforceTests.Fixtures;
using Hub.Managers.APIManagers.Transmitters.Restful;
using System;
using Data.Interfaces.DataTransferObjects.Helpers;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    public class Create_Contact_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async Task Create_Contact_Initial_Configuration_Check_Crate_Structure()
        {
            //Act
            var initialConfigActionDto = await PerformInitialConfiguration();

            //Assert
            Assert.IsNotNull(initialConfigActionDto.CrateStorage,
                "Initial Configuration of Create Contact activity contains no crate storage");

            AssertConfigurationControls(Crate.FromDto(initialConfigActionDto.CrateStorage));
        }

        [Test, Category("intergration.terminalSalesforce")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException)
        )]
        public async Task Create_Contact_Initial_Configuration_Without_AuthToken_Exception_Thrown()
        {
            //Arrange
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create contact action DTO
            var dataDTO = HealthMonitor_FixtureData.Create_Contact_v1_InitialConfiguration_Fr8DataDTO();
            dataDTO.ActivityDTO.AuthToken = null;

            //Act
            //perform post request to terminal and return the result
            await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, dataDTO);
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async Task Create_Contact_Run_With_NoAuth_Check_NoAuthProvided_Error()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            initialConfigActionDto = SetLastName(initialConfigActionDto);
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
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

        [Test, Category("intergration.terminalSalesforce"), Ignore("Ignored due to the introduction of new Activate checking logic.")]
        public async Task Create_Contact_Run_With_NoLastName_Check_NoLastNameProvided_Error()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            initialConfigActionDto.AuthToken = HealthMonitor_FixtureData.Salesforce_AuthToken().Result;
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);

            //Assert
            Assert.IsNotNull(responseOperationalState);
            var curOperationalState =
                Crate.FromDto(responseOperationalState.CrateStorage).CratesOfType<OperationalStateCM>().Single().Content;
            ErrorDTO errorMessage;
            curOperationalState.CurrentActivityResponse.TryParseErrorDTO(out errorMessage);
            Assert.AreEqual("No last name found in activity.", errorMessage.Message, "Action works without account name");
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async Task Create_Contact_Run_With_ValidParameter_Check_PayloadDto_OperationalState()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            initialConfigActionDto = SetLastName(initialConfigActionDto);
            initialConfigActionDto.AuthToken = HealthMonitor_FixtureData.Salesforce_AuthToken().Result;
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
            
            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);

            //Assert
            Assert.IsNotNull(responseOperationalState);
        }

        /// <summary>
        /// Performs Initial Configuration request of Create Contact action
        /// </summary>
        private async Task<ActivityDTO> PerformInitialConfiguration()
        {
            //get the terminal configure URL
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create account action DTO
            var requestActionDTO = HealthMonitor_FixtureData.Create_Contact_v1_InitialConfiguration_Fr8DataDTO();

            //perform post request to terminal and return the result
            var resultActionDto = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, requestActionDTO);

            using (var crateStorage = Crate.GetUpdatableStorage(resultActionDto))
            {
                var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
                controls.Controls.OfType<TextSource>().ToList().ForEach(ctl => ctl.ValueSource = "specific");
            }

            return resultActionDto;
        }

        private void AssertConfigurationControls(ICrateStorage curActionCrateStorage)
        {
            var configurationControls = curActionCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();

            Assert.AreEqual(20, configurationControls.Content.Controls.Count,
                "Create Contact does not contain the required 3 fields.");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("FirstName")),
                "Create Contact activity does not have First Name control");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("LastName")),
                "Create Contact does not have Last Name control");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("MobilePhone")),
                "Create Contact does not have Mobile Phone control");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("Email")),
                "Create Contact does not have Email control");

            //@AlexAvrutin: Commented this since the textbox here do not require requestConfig event. 
            //Assert.IsFalse(configurationControls.Content.Controls.Any(ctrl => ctrl.Events.Count != 1),
            //    "Create Contact controls are not subscribed to on Change events");
        }

        private ActivityDTO SetLastName(ActivityDTO curActivityDto)
        {
            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDto))
            {
                var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();

                var targetUrlTextBox = (TextSource)controls.Controls[1];
                targetUrlTextBox.ValueSource = "specific";
                targetUrlTextBox.TextValue = "IntegrationTestContact";
            }

            return curActivityDto;
        }
    }
}
