using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalSalesforce;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using terminalSalesforceTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;

namespace terminalSalesforceTests.Actions
{
    [TestFixture]
    [Category("terminalSalesforceTests")]
    public class Save_To_SalesforceDotCom_v1Tests : BaseTest
    {
        private Save_To_SalesforceDotCom_v1 _saveToSFDotCom_v1;

        public override void SetUp()
        {
            base.SetUp();

            TerminalBootstrapper.ConfigureTest();
            TerminalSalesforceStructureMapBootstrapper.ConfigureDependencies(TerminalSalesforceStructureMapBootstrapper.DependencyType.TEST);

            PayloadDTO testPayloadDTO = new PayloadDTO(new Guid());

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(testPayloadDTO))
            {
                crateStorage.Add(Crate.FromContent("Operational Status", new OperationalStateCM()));
            }

            Mock<IHubCommunicator> hubCommunicatorMock = new Mock<IHubCommunicator>(MockBehavior.Default);
            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(testPayloadDTO));
            ObjectFactory.Container.Inject(typeof(IHubCommunicator), hubCommunicatorMock.Object);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());
            FieldDTO testField = new FieldDTO("Account", "TestAccount");
            salesforceIntegrationMock.Setup(
                s => s.GetProperties(SalesforceObjectType.Account, It.IsAny<AuthorizationTokenDO>(), false))
                .Returns(() => Task.FromResult(new List<FieldDTO> { testField }));

            salesforceIntegrationMock.Setup(
                s => s.Query(SalesforceObjectType.Account, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<AuthorizationTokenDO>()))
                .Returns(() => Task.FromResult(new StandardTableDataCM()));

            _saveToSFDotCom_v1 = new Save_To_SalesforceDotCom_v1();
        }

        [Test, Category("terminalSalesforceTests.Save_To_SalesforceDotCom.Configure")]
        public async Task Configure_InitialConfig_CheckOnlyOneDDLBPresent()
        {
            //Act
            var result = await PerformInitialConfig();

            //Assert
            AssertConfigControls(ObjectFactory.GetInstance<ICrateManager>().GetStorage(result));
        }

        [Test, Category("terminalSalesforceTests.Save_To_SalesforceDotCom.Configure")]
        public async Task Configure_FollowUpConfig_CheckTextSourceControlsCreated()
        {
            //Arrange
            var result = await PerformInitialConfig();

            //Act
            result = await _saveToSFDotCom_v1.Configure(result, await FixtureData.Salesforce_AuthToken());

            //Assert
            var storage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(result);
            Assert.IsNotNull(storage, "Follow up config storage is null in Save to SF.com activity");

            var listOfControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single().Controls;
            Assert.IsTrue(listOfControls.Count > 1, "No Text Sources are created in follow up config of Save to SF.com activity");
            Assert.IsTrue(listOfControls.OfType<TextSource>().Count() > 0, "No Text Sources are created in follow up config of Save to SF.com activity");
        }

        [Test, Category("terminalSalesforceTests.Save_To_SalesforceDotCom.Activate")]
        public async Task Activate_CheckErrorMessageOnRequiredFields()
        {
            //Arrange
            //perform initial and follow up config
            var result = await PerformInitialConfig();
            var authToken = await FixtureData.Salesforce_AuthToken();
            result = await _saveToSFDotCom_v1.Configure(result, authToken);

            //Act
            result = await _saveToSFDotCom_v1.Activate(result, authToken);

            //Asser
            var listOfRequiredControls = ObjectFactory.GetInstance<ICrateManager>().GetStorage(result)
                                            .CratesOfType<StandardConfigurationControlsCM>().Single().Content
                                            .Controls.Where(c => c.Name.Equals("LastName") || c.Name.Equals("Company"));
            Assert.IsTrue(listOfRequiredControls.Count() == 2, "There are no required controls in Save to SF Activate");
            Assert.IsFalse(listOfRequiredControls.Any(c => string.IsNullOrEmpty(c.ErrorMessage)), "There are some required controls error message is not set");
        }

        [Test, Category("terminalSalesforceTests.Save_To_SalesforceDotCom.Activate")]
        public async Task Run_CheckTheObjectIsCreated()
        {
            //Arrange
            //perform initial and follow up config, activate
            var result = await PerformInitialConfig();

            var authToken = await FixtureData.Salesforce_AuthToken();
            result = await _saveToSFDotCom_v1.Configure(result, authToken);
            result = await _saveToSFDotCom_v1.Activate(result, authToken);

            //make last name as Unit and Company Name as Test
            using (var storage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(result))
            {
                var requiredControls = ObjectFactory.GetInstance<ICrateManager>().GetStorage(result)
                                            .CratesOfType<StandardConfigurationControlsCM>().Single().Content
                                            .Controls;

                var lastNameControl = (TextSource)requiredControls.Single(c => c.Name.Equals("LastName"));
                lastNameControl.ValueSource = "specific";
                lastNameControl.TextValue = "Unit";

                var companyControl = (TextSource)requiredControls.Single(c => c.Name.Equals("Company"));
                lastNameControl.ValueSource = "specific";
                lastNameControl.TextValue = "Text";
            }

            //Act
            var payload = await _saveToSFDotCom_v1.Run(result, new Guid(), authToken);

            //Assert
            var newlyCreatedLead = ObjectFactory.GetInstance<ICrateManager>()
                                    .GetStorage(payload).CratesOfType<StandardPayloadDataCM>()
                                    .Single().Content.PayloadObjects[0].PayloadObject;

            Assert.IsNotNull(newlyCreatedLead, "Lead is not saved successfully in Save to SF.com");
            Assert.IsTrue(newlyCreatedLead.Count == 1, "Lead is not saved successfully in Save to SF.com");
            Assert.IsTrue(!string.IsNullOrEmpty(newlyCreatedLead[0].Value), "Lead is not saved successfully in Save to SF.com");

            var isDeleted = await new SalesforceManager().Delete(SalesforceObjectType.Lead, newlyCreatedLead[0].Value, authToken);
            Assert.IsTrue(isDeleted, "The newly created lead is not deleted upon completion");
        }

        private async Task<ActivityDO> PerformInitialConfig()
        {
            var activityDO = FixtureData.SaveToSalesforceTestActivityDO1();

            //Act
            var result = await _saveToSFDotCom_v1.Configure(activityDO, await FixtureData.Salesforce_AuthToken());

            AssertConfigControls(ObjectFactory.GetInstance<ICrateManager>().GetStorage(result));

            using (var storage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(result))
            {
                var ddlb = (DropDownList)storage.CratesOfType<StandardConfigurationControlsCM>().Single().Content.Controls[0];
                ddlb.selectedKey = "Lead";
            }

            return result;
        }

        private void AssertConfigControls(ICrateStorage storage)
        {
            Assert.IsNotNull(storage, "Save to Salesforce.com activty crate storage is null.");

            var configControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControls, "There is no config controls for Save to Salesforce.com activity");
            Assert.AreEqual(1, configControls.Controls.Count, "There is more than one config control at initial config of Save to Salesforce.com");

            var dropDownList = configControls.Controls.OfType<DropDownList>().Single();
            Assert.IsNotNull(dropDownList, "SF Object Type drop down list is NULL in Save to SF.com");
            Assert.IsNotNull(dropDownList.ListItems, "List items are not populated with supported SF object types.");
            Assert.IsTrue(dropDownList.ListItems.Count > 0, "List items are not populated with supported SF object types.");
        }
    }
}