using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.WebSockets;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Salesforce.Force;
using StructureMap;
using TerminalBase.Infrastructure;
using terminalSalesforce;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforceTests.Fixtures;
using UtilitiesTesting;

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
                s => s.GetFields("Account", It.IsAny<AuthorizationTokenDO>(), false))
                .Returns(() => Task.FromResult((IList<FieldDTO>)new List<FieldDTO> { testField }));

            salesforceIntegrationMock.Setup(
                s => s.GetObjectByQuery("Account", It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<AuthorizationTokenDO>()))
                .Returns(() => Task.FromResult(new StandardPayloadDataCM()));

            _getData_v1 = new Get_Data_v1();
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Configure")]
        public async Task Configure_InitialConfig_CheckControlsCrate()
        {
            //Arrange
            var activityDO = FixtureData.GetFileListTestActivityDO1();

            //Act
            var result = await _getData_v1.Configure(activityDO, await FixtureData.Salesforce_AuthToken());

            //Assert
            var stroage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(result);
            Assert.AreEqual(1, stroage.Count, "Number of configuration crates not populated correctly");

            var configControlCM = stroage.CratesOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControlCM, "Configuration controls is not present");

            Assert.AreEqual(3, configControlCM.Content.Controls.Count, "Number of configuration controls are not correct");
            Assert.IsTrue(configControlCM.Content.Controls.Any(control => control.Name.Equals("WhatKindOfData")), "WhatKindOfData DDLB is not present");
            Assert.IsTrue(configControlCM.Content.Controls.Any(control => control.Name.Equals("SelectedQuery")), "SelectedQuery DDLB is not present");
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Configure")]
        public async Task Configure_FollowUpConfig_CheckObjectFields()
        {
            //Arrange
            var authToken = await FixtureData.Salesforce_AuthToken();
            var activityDO = FixtureData.GetFileListTestActivityDO1();
            activityDO = await _getData_v1.Configure(activityDO, authToken);
            activityDO = SelectSalesforceAccount(activityDO);

            Mock<ISalesforceManager> salesforceIntegrationMock = Mock.Get(ObjectFactory.GetInstance<ISalesforceManager>());

            //Act
            activityDO = await _getData_v1.Configure(activityDO, authToken);

            //Assert
            var stroage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(activityDO);
            Assert.AreEqual(3, stroage.Count, "Number of configuration crates not populated correctly");

            Assert.AreEqual(stroage.CratesOfType<TypedFieldsCM>()
                    .Single(c => c.Label.Equals("Queryable Criteria"))
                    .Content.Fields.Count, 1, "Queryable Criteria is NOT filled with invalid data");

            salesforceIntegrationMock.Verify(s => s.GetFields("Account", It.IsAny<AuthorizationTokenDO>(), false), Times.Exactly(1));
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Run")]
        public async Task Run_Check_PayloadDTO_ForObjectData()
        {
            //Arrange
            var authToken = await FixtureData.Salesforce_AuthToken();
            var activityDO = FixtureData.GetFileListTestActivityDO1();

            //perform initial configuration
            activityDO = await _getData_v1.Configure(activityDO, authToken);
            activityDO = SelectSalesforceAccount(activityDO);
            //perform follow up configuration
            activityDO = await _getData_v1.Configure(activityDO, authToken);

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(activityDO))
            {
                crateStorage.CratesOfType<StandardConfigurationControlsCM>()
                    .Single()
                    .Content.Controls.Single(control => control.Type == ControlTypes.QueryBuilder)
                    //.Value = JsonConvert.SerializeObject(new FilterDataDTO() {Conditions = new List<FilterConditionDTO>()});
                    .Value = JsonConvert.SerializeObject(new List<FilterConditionDTO>());
            }

            //Act
            var resultPayload = await _getData_v1.Run(activityDO, new Guid(), authToken);

            //Assert
            var stroage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(resultPayload);
            Assert.AreEqual(2, stroage.Count, "Number of Payload crates not populated correctly");

            Assert.IsNotNull(stroage.CratesOfType<StandardPayloadDataCM>()
                    .Single(c => c.Label.Equals("Salesforce Objects")), "Not able to get the required salesforce object");
        }

        private ActivityDO SelectSalesforceAccount(ActivityDO curActivityDO)
        {
            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(curActivityDO))
            {
                var configControls = crateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
                configControls.Content.Controls.Where(control => control.Name != null && control.Name.Equals("WhatKindOfData"))
                    .Select(control => control as DropDownList)
                    .Single()
                    .selectedKey = "Account";
            }
            return curActivityDO;
        }
    }
}
