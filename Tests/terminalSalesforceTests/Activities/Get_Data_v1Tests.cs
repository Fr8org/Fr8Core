using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalSalesforce;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforceTests.Fixtures;
using Fr8.Testing.Unit;
namespace terminalSalesforceTests.Actions
{
    [TestFixture]
    [Category("terminalSalesforceTests")]
    public class Get_Data_v1Tests : BaseTest
    {
        private Get_Data_v1 _getData_v1;

        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            ObjectFactory.Configure(x => x.AddRegistry<TerminalSalesforceStructureMapBootstrapper.TestMode>());
            PayloadDTO testPayloadDTO = new PayloadDTO(new Guid());

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(testPayloadDTO))
            {
                crateStorage.Add(Crate.FromContent("Operational Status", new OperationalStateCM()));
            }

            Mock<IHubCommunicator> hubCommunicatorMock = new Mock<IHubCommunicator>(MockBehavior.Default);
            hubCommunicatorMock.Setup(h => h.GetPayload(It.IsAny<Guid>()))
                .Returns(() => Task.FromResult(testPayloadDTO));
            ObjectFactory.Container.Inject(typeof(IHubCommunicator), hubCommunicatorMock.Object);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());
            FieldDTO testField = new FieldDTO("Account") {Label = "TestAccount"};
            salesforceIntegrationMock.Setup(
                s => s.GetProperties(SalesforceObjectType.Account, It.IsAny<AuthorizationToken>(), false,null))
                .Returns(() => Task.FromResult(new List<FieldDTO> { testField }));

            salesforceIntegrationMock.Setup(
                s => s.Query(SalesforceObjectType.Account, It.IsAny<IList<FieldDTO>>(), It.IsAny<string>(), It.IsAny<AuthorizationToken>()))
                .Returns(() => Task.FromResult(new StandardTableDataCM()));

            _getData_v1 = New<Get_Data_v1>();
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Configure")]
        public async Task Configure_InitialConfig_CheckControlsCrate()
        {
            //Arrange
            var activityContext = await FixtureData.GetFileListTestActivityContext1();

            //Act
            await _getData_v1.Configure(activityContext);

            //Assert
            var storage = activityContext.ActivityPayload.CrateStorage;
            Assert.AreEqual(2, storage.Count, "Number of configuration crates not populated correctly");
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>(), "Configuration controls is not present");
            Assert.IsNotNull(storage.FirstCrateOrDefault<CrateDescriptionCM>(), "There is no crate with runtime crates descriptions in activity storage");
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Configure")]
        public async Task Configure_FollowUpConfig_CheckObjectFields()
        {
            // Arrange
            var activityContext = await FixtureData.GetFileListTestActivityContext2();
            await _getData_v1.Configure(activityContext);
            SelectSalesforceAccount(activityContext);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());

            // Act
            await _getData_v1.Configure(activityContext);

            // Assert
            var storage = activityContext.ActivityPayload.CrateStorage;
            Assert.AreEqual(5, storage.Count, "Number of configuration crates not populated correctly");

            // Assert.IsNotNull(storage.FirstCrateOrDefault<TypedFieldsCM>(x => x.Label == Get_Data_v1.QueryFilterCrateLabel), 
            //                  "There is not crate with query fields descriptions and expected label in activity storage");
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>(), "There is not crate with controls in activity storage");
            Assert.IsNotNull(storage.FirstCrateOrDefault<CrateDescriptionCM>(), "There is no crate with runtime crates descriptions in activity storage");
            Assert.IsNotNull(storage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == "Queryable Criteria"),
                             "There is no crate with field descriptions of selected Salesforce object in activity storage");

            salesforceIntegrationMock.Verify(s => s.GetProperties(SalesforceObjectType.Account, It.IsAny<AuthorizationToken>(), false,null), Times.Exactly(1));
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Run")]
        public async Task Run_Check_PayloadDTO_ForObjectData()
        {
            //Arrange
            var authToken = await FixtureData.Salesforce_AuthToken();
            var activityContext = await FixtureData.GetFileListTestActivityContext1();
            var executionContext = new ContainerExecutionContext
            {
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };

            //perform initial configuration
            await _getData_v1.Configure(activityContext);
            activityContext = SelectSalesforceAccount(activityContext);
            //perform follow up configuration
            await _getData_v1.Configure(activityContext);

            activityContext.ActivityPayload.CrateStorage.CratesOfType<StandardConfigurationControlsCM>()
                .Single()
                .Content.Controls.Single(control => control.Type == ControlTypes.QueryBuilder)
                //.Value = JsonConvert.SerializeObject(new FilterDataDTO() {Conditions = new List<FilterConditionDTO>()});
                .Value = JsonConvert.SerializeObject(new List<FilterConditionDTO>());

            //Act
            await _getData_v1.Run(activityContext, executionContext);
            //Assert
            var stroage = executionContext.PayloadStorage;
            Assert.AreEqual(2, stroage.Count, "Number of Payload crates not populated correctly");

            Assert.IsNotNull(stroage.CratesOfType<StandardTableDataCM>().Single(), "Not able to get the required salesforce object");
        }

        private ActivityContext SelectSalesforceAccount(ActivityContext activityContext)
        {
            var controlsCrate = activityContext.ActivityPayload.CrateStorage.FirstCrate<StandardConfigurationControlsCM>();
            var activityUi = new Get_Data_v1.ActivityUi().ClonePropertiesFrom(controlsCrate.Content) as Get_Data_v1.ActivityUi;
            activityUi.SalesforceObjectSelector.selectedKey = "Account";
            controlsCrate.Content.ClonePropertiesFrom(activityUi);
            return activityContext;
        }
    }
}
