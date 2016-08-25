using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Integration;
using NUnit.Framework;
using StructureMap;
using terminalSalesforceTests.Fixtures;
using terminalSalesforce.Actions;
using terminalSalesforce.Services;
using terminalSalesforce.Infrastructure;
using System;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    public class Post_To_Chatter_v1Tests : BaseSalesforceIntegrationTest
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
        public async Task Post_To_Chatter_Initial_Configuration_Without_AuthToken_Should_Fail()
        {
            //Arrange
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create lead action DTO
            var dataDTO = HealthMonitor_FixtureData.Post_To_Chatter_v1_InitialConfiguration_Fr8DataDTO();
            dataDTO.ActivityDTO.AuthToken = null;

            //Act
            //perform post request to terminal and return the result
            var response = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, dataDTO);
            Assert.NotNull(response);
            Assert.NotNull(response.CrateStorage);
            Assert.NotNull(response.CrateStorage.Crates);
            Assert.True(response.CrateStorage.Crates.Any(x => x.ManifestType == "Standard Authentication"));
        }

        [Test, Category("intergration.terminalSalesforce")]
        public async Task Post_To_Chatter_Run_With_NoAuth_Check_NoAuthProvided_Error()
        {
            //Arrange
            var initialConfigActionDto = await PerformInitialConfiguration();
            var dataDTO = new Fr8DataDTO { ActivityDTO = initialConfigActionDto };
            AddOperationalStateCrate(dataDTO, new OperationalStateCM());
        
            //Act
            var responseOperationalState = await HttpPostAsync<Fr8DataDTO, PayloadDTO>(GetTerminalRunUrl(), dataDTO);
            var operationalState = Crate.GetStorage(responseOperationalState)
                                         .CratesOfType<OperationalStateCM>()
                                         .SingleOrDefault()
                                         ?.Content;
            Assert.AreEqual("Error", operationalState.CurrentActivityResponse.Type, "Activity response doesn't contain error");
            Assert.IsTrue(operationalState.CurrentActivityResponse.Body.Contains("AUTH_TOKEN_NOT_PROVIDED_OR_INVALID"), "Activity response error doesn't contain info about the missing auth token fact");
            //Assert.AreEqual(operationalState.CurrentActivityResponse.Type);

        }

        [Test, Category("intergration.terminalSalesforce")]
        public async Task Post_To_Chatter_Run_With_ValidParameter_Check_PayloadDto_OperationalState()
        {
            //Arrange
            var authToken = terminalIntegrationTests.Fixtures.HealthMonitor_FixtureData.Salesforce_AuthToken().Result;
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
            Assert.IsTrue(await _container.GetInstance<SalesforceManager>().Delete(SalesforceObjectType.FeedItem, 
                newFeedIdCrate.Content.PayloadObjects[0].PayloadObject[0].Value, new AuthorizationToken { Token = authToken.Token, AdditionalAttributes = authToken.AdditionalAttributes }), "Test feed created is not deleted");
        }
        
        private async Task<ActivityDTO> PerformInitialConfiguration()
        {
            //get the terminal configure URL
            string terminalConfigureUrl = GetTerminalConfigureUrl();

            //prepare the create account action DTO
            var requestActionDTO = HealthMonitor_FixtureData.Post_To_Chatter_v1_InitialConfiguration_Fr8DataDTO();

            //perform post request to terminal and return the result
            var resultActionDto = await HttpPostAsync<Fr8DataDTO, ActivityDTO>(terminalConfigureUrl, requestActionDTO);
            using (var crateStorage = Crate.GetUpdatableStorage(resultActionDto))
            {
                crateStorage.UpdateControls<Post_To_Chatter_v1.ActivityUi>(x =>
            {
                x.UseUserOrGroupOption.Selected = true;
                var selectedUser = x.UserOrGroupSelector.ListItems.First(y => y.Key == "Fr8 Admin");
                x.UserOrGroupSelector.selectedKey = selectedUser.Key;
                x.UserOrGroupSelector.Value = selectedUser.Value;
                x.FeedTextSource.ValueSource = "specific";
                x.FeedTextSource.TextValue = "IntegrationTestFeed";
            });
            }
            return resultActionDto;
        }
    }
}
