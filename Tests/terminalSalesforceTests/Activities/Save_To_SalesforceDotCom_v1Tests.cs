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
using Fr8Data.Managers;
using TerminalBase.Models;

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
            ObjectFactory.Configure(x => x.For<ICrateManager>().Use(CrateManager));
            using (var crateStorage = CrateManager.GetUpdatableStorage(testPayloadDTO))
            {
                crateStorage.Add(Crate.FromContent("Operational Status", new OperationalStateCM()));
            }

            Mock<IHubCommunicator> hubCommunicatorMock = new Mock<IHubCommunicator>(MockBehavior.Default);
            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(testPayloadDTO));
            ObjectFactory.Container.Inject(typeof(IHubCommunicator), hubCommunicatorMock.Object);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());
            FieldDTO testField = new FieldDTO("Account", "TestAccount");
            salesforceIntegrationMock.Setup(
                s => s.GetProperties(SalesforceObjectType.Account, It.IsAny<AuthorizationToken>(), false, null))
                .Returns(() => Task.FromResult(new List<FieldDTO> { testField }));

            salesforceIntegrationMock.Setup(
                s => s.Query(SalesforceObjectType.Account, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<AuthorizationToken>()))
                .Returns(() => Task.FromResult(new StandardTableDataCM()));

            _saveToSFDotCom_v1 = new Save_To_SalesforceDotCom_v1();
        }

        [Test, Category("terminalSalesforceTests.Save_To_SalesforceDotCom.Configure")]
        public async Task Configure_InitialConfig_CheckOnlyOneDDLBPresent()
        {
            //Act
            var result = await PerformInitialConfig();

            //Assert
            AssertConfigControls(result.ActivityPayload.CrateStorage);
        }

        [Test, Category("terminalSalesforceTests.Save_To_SalesforceDotCom.Configure")]
        public async Task Configure_FollowUpConfig_CheckTextSourceControlsCreated()
        {
            //Arrange
            var result = await PerformInitialConfig();

            //Act
            await _saveToSFDotCom_v1.Configure(result);

            //Assert
            var storage = result.ActivityPayload.CrateStorage;
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
            await _saveToSFDotCom_v1.Configure(result);
            //Act
            await _saveToSFDotCom_v1.Activate(result);

            var crateStorage = result.ActivityPayload.CrateStorage;

            //Asser
            var listOfRequiredControls = crateStorage
                                            .CratesOfType<StandardConfigurationControlsCM>().Single().Content
                                            .Controls.Where(c => c.Name.Equals("LastName") || c.Name.Equals("Company"));
            Assert.IsTrue(listOfRequiredControls.Count() == 2, "There are no required controls in Save to SF Activate");

            var validationCm = crateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();

            Assert.IsNotNull(validationCm, "Validation results crate was not found");

            foreach (var control in listOfRequiredControls)
            {
                var ctrl = control;

                if (!validationCm.ValidationErrors.Any(x => x.ControlNames.Any(y => y == ctrl.Name)))
                {
                    Assert.Fail($"Control {control.Name} has no associated errors.");
                }
            }
        }

        [Test, Category("terminalSalesforceTests.Save_To_SalesforceDotCom.Activate")]
        public async Task Run_CheckTheObjectIsCreated()
        {
            //Arrange
            //perform initial and follow up config, activate
            var result = await PerformInitialConfig();
            var executionContext = new ContainerExecutionContext();

            var authToken = await FixtureData.Salesforce_AuthToken();
            var authorizationToken = new AuthorizationToken
            {
                Token = authToken.Token
            };
            await _saveToSFDotCom_v1.Configure(result);
            await _saveToSFDotCom_v1.Activate(result);

            //make last name as Unit and Company Name as Test
            var requiredControls = result.ActivityPayload.CrateStorage
                                        .CratesOfType<StandardConfigurationControlsCM>().Single().Content
                                        .Controls;

            var lastNameControl = (TextSource)requiredControls.Single(c => c.Name.Equals("LastName"));
            lastNameControl.ValueSource = "specific";
            lastNameControl.TextValue = "Unit";

            var companyControl = (TextSource)requiredControls.Single(c => c.Name.Equals("Company"));
            companyControl.ValueSource = "specific";
            companyControl.TextValue = "Text";

            //Act
            await _saveToSFDotCom_v1.Run(result, executionContext);
            var payload = result.ActivityPayload.CrateStorage;
            //Assert
            var newlyCreatedLead = payload.CratesOfType<StandardPayloadDataCM>()
                                    .Single().Content.PayloadObjects[0].PayloadObject;

            Assert.IsNotNull(newlyCreatedLead, "Lead is not saved successfully in Save to SF.com");
            Assert.IsTrue(newlyCreatedLead.Count == 1, "Lead is not saved successfully in Save to SF.com");
            Assert.IsTrue(!string.IsNullOrEmpty(newlyCreatedLead[0].Value), "Lead is not saved successfully in Save to SF.com");

            var isDeleted = await new SalesforceManager().Delete(SalesforceObjectType.Lead, newlyCreatedLead[0].Value, authorizationToken);
            Assert.IsTrue(isDeleted, "The newly created lead is not deleted upon completion");
        }

        private async Task<ActivityContext> PerformInitialConfig()
        {
            var activityContext = await FixtureData.SaveToSalesforceTestActivityDO1();
            //Act
            await _saveToSFDotCom_v1.Configure(activityContext);

            AssertConfigControls(activityContext.ActivityPayload.CrateStorage);

            var ddlb = (DropDownList)activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single().Content.Controls[0];
            ddlb.selectedKey = "Lead";

            return activityContext;
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