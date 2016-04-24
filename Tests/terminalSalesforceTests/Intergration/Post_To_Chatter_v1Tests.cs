using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using NUnit.Framework;
using terminalSalesforceTests.Fixtures;
using terminalSalesforce.Services;
using terminalSalesforce.Infrastructure;
using Data.Entities;
using terminalSalesforce.Activities;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    public class Post_To_Chatter_v1Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async Task Post_To_Chatter_Initial_Configuration_Check_Crate_Structure()
        {
            //Act
            var initialConfigActionDto = await PerformInitialConfiguration();

            //Assert
            Assert.IsNotNull(initialConfigActionDto.CrateStorage, "Initial Configuration of Post To Chatter activity contains no crate storage");
            var storage = Crate.GetStorage(initialConfigActionDto);
            Assert.AreEqual(2, storage.Count, "Number of configuration crates not populated correctly");
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>(), "Configuration controls crate is not found in activity storage");
            Assert.IsNotNull(storage.FirstCrateOrDefault<CrateDescriptionCM>(), "Crate with runtime crates desriptions is not found in activity storage");
        }

        [Test, Category("intergration.terminalSalesforce")]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException)
        )]
        public async Task Post_To_Chatter_Initial_Configuration_Without_AuthToken_Exception_Thrown()
        {
            //Arrange
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create lead action DTO
            var dataDTO = HealthMonitor_FixtureData.Post_To_Chatter_v1_InitialConfiguration_Fr8DataDTO();
            dataDTO.ActivityDTO.AuthToken = null;

            //Act
            //perform post request to terminal and return the result
            await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, dataDTO);
        }

        [Test, Category("intergration.terminalSalesforce"), Ignore]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException)
        )]
        public async Task Post_To_Chatter_Run_With_NoAuth_Check_NoAuthProvided_Error()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async Task Post_To_Chatter_Run_With_ValidParameter_Check_PayloadDto_OperationalState()
        {
            //Arrange
            var authToken = HealthMonitor_FixtureData.Salesforce_AuthToken().Result;
            var initialConfigActionDto = await PerformInitialConfiguration();
            initialConfigActionDto.AuthToken = authToken;
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());

            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);

            //Assert
            Assert.IsNotNull(responseOperationalState);
            var newFeedIdCrate = Crate.GetStorage(responseOperationalState)
                                         .CratesOfType<StandardPayloadDataCM>()
                                         .SingleOrDefault();
            Assert.IsNotNull(newFeedIdCrate, "Feed is not created");
            Assert.IsTrue(await new SalesforceManager().Delete(SalesforceObjectType.FeedItem, 
                newFeedIdCrate.Content.PayloadObjects[0].PayloadObject[0].Value, new AuthorizationTokenDO { Token = authToken.Token, AdditionalAttributes = authToken.AdditionalAttributes }), "Test feed created is not deleted");
        }

        private async Task<ActivityDTO> PerformInitialConfiguration()
        {
            //get the terminal configure URL
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create account action DTO
            var requestActionDTO = HealthMonitor_FixtureData.Post_To_Chatter_v1_InitialConfiguration_Fr8DataDTO();

            //perform post request to terminal and return the result
            var resultActionDto = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, requestActionDTO);
            resultActionDto.UpdateControls<Post_To_Chatter_v1.ActivityUi>(x =>
            {
                x.UseUserOrGroupOption.Selected = true;
                var selectedUser = x.UserOrGroupSelector.ListItems.First(y => y.Key == "Fr8 Admin");
                x.UserOrGroupSelector.selectedKey = selectedUser.Key;
                x.UserOrGroupSelector.Value = selectedUser.Value;
                x.FeedTextSource.ValueSource = "specific";
                x.FeedTextSource.TextValue = "IntegrationTestFeed";
            });
            return resultActionDto;
        }
    }
}
