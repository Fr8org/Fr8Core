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
            Assert.IsNotNull(initialConfigActionDto.CrateStorage,
                "Initial Configuration of Post To Chatter activity contains no crate storage");

            AssertConfigurationControls(Crate.FromDto(initialConfigActionDto.CrateStorage));
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
            Assert.IsTrue(responseOperationalState.CrateStorage.Crates.Any(c => c.Label.Equals("Newly Created Salesforce Feed")), "Feed is not created");

            var newFeedIdCrate = Crate.GetStorage(responseOperationalState)
                                         .CratesOfType<StandardPayloadDataCM>()
                                         .Single(c => c.Label.Equals("Newly Created Salesforce Feed"));

            Assert.IsTrue(
                await SalesforceTestHelper.DeleteObject(authToken, "FeedItem", newFeedIdCrate.Content.PayloadObjects[0].PayloadObject[0].Value),
                "Test lead created is not deleted");
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
                var controls = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
                controls.Controls.OfType<TextSource>().ToList().ForEach(ctl => { ctl.ValueSource = "specific"; ctl.TextValue = "IntegrationTestFeed"; });

                var chatterObjects = crateStorage.CratesOfType<FieldDescriptionsCM>().Single(crate => crate.Label.Equals("AvailableChatters"));
                controls.Controls.OfType<DropDownList>().ToList().ForEach(ctl => 
                                            {
                                                ctl.selectedKey = chatterObjects.Content.Fields[0].Key;
                                                ctl.Value = chatterObjects.Content.Fields[0].Value; });
            }

            return resultActionDto;
        }

        private void AssertConfigurationControls(ICrateStorage curActionCrateStorage)
        {
            var availableChattersCrate = curActionCrateStorage.CratesOfType<FieldDescriptionsCM>().Single(crate => crate.Label.Equals("AvailableChatters"));

            Assert.IsNotNull(availableChattersCrate, "Available Chatters is not populated in Initial Configuration");
            Assert.IsTrue(availableChattersCrate.Content.Fields.Count > 0, "Available Chatters are not retrieved from the Salesforce");

            var configurationControls = curActionCrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();

            Assert.AreEqual(2, configurationControls.Content.Controls.Count,
                "Post To Chatter does not contain the required fields.");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("WhatKindOfChatterObject")),
                "Post To Chatter action does not have WhatKindOfChatterObject control");

            Assert.IsTrue(configurationControls.Content.Controls.Any(ctrl => ctrl.Name.Equals("FeedTextItem")),
                "Post To Chatter does not have FeedTextItem control");
        }
    }
}
