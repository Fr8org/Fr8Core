using System.Linq;
using Data.Control;
using Data.Entities;
using Data.Interfaces.Manifests;
using Hub.Managers;
using NUnit.Framework;
using StructureMap;
using TerminalBase.Infrastructure;
using terminalSalesforce;
using terminalSalesforce.Actions;
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

            ObjectFactory.Configure(cfg => cfg.For<IHubCommunicator>().Use<DefaultHubCommunicator>());

            _getData_v1 = new Get_Data_v1();
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Configure")]
        public async void Configure_InitialConfig_CheckControlsCrate()
        {
            //Arrange
            var actionDO = FixtureData.GetFileListTestActionDO1();

            //Act
            var result = await _getData_v1.Configure(actionDO, FixtureData.Salesforce_AuthToken());

            //Assert
            var stroage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(result);
            Assert.AreEqual(3, stroage.Count, "Number of configuration crates not populated correctly");

            var configControlCM = stroage.CratesOfType<StandardConfigurationControlsCM>().Single();
            Assert.IsNotNull(configControlCM, "Configuration controls is not present");

            Assert.AreEqual(2, configControlCM.Content.Controls.Count, "Number of configuration controls are not correct");
            Assert.IsTrue(configControlCM.Content.Controls.Any(control => control.Name.Equals("WhatKindOfData")), "WhatKindOfData DDLB is not present");
            Assert.IsTrue(configControlCM.Content.Controls.Any(control => control.Name.Equals("SelectedFilter")), "SelectedFilter DDLB is not present");

            Assert.AreEqual(0,
                stroage.CratesOfType<StandardDesignTimeFieldsCM>()
                    .Single(c => c.Label.Equals("Queryable Criteria"))
                    .Content.Fields.Count, "Queryable Criteria is filled with invalid data");
        }

        [Test, Category("terminalSalesforceTests.Get_Data.Configure")]
        public async void Configure_FollowUpConfig_CheckObjectFields()
        {
            //Arrange
            var actionDO = FixtureData.GetFileListTestActionDO1();
            actionDO = await _getData_v1.Configure(actionDO, FixtureData.Salesforce_AuthToken());
            actionDO = SelectSalesforceAccount(actionDO);

            //Act
            actionDO = await _getData_v1.Configure(actionDO, FixtureData.Salesforce_AuthToken());

            //Assert
            var stroage = ObjectFactory.GetInstance<ICrateManager>().GetStorage(actionDO);
            Assert.AreEqual(3, stroage.Count, "Number of configuration crates not populated correctly");

            Assert.Greater(stroage.CratesOfType<StandardDesignTimeFieldsCM>()
                    .Single(c => c.Label.Equals("Queryable Criteria"))
                    .Content.Fields.Count, 0, "Queryable Criteria is NOT filled with invalid data");
        }

        private ActionDO SelectSalesforceAccount(ActionDO curActionDO)
        {
            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(curActionDO))
            {
                var configControls = updater.CrateStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
                configControls.Content.Controls.Where(control => control.Name.Equals("WhatKindOfData"))
                    .Select(control => control as DropDownList)
                    .Single()
                    .selectedKey = "Account";
            }
            return curActionDO;
        }
    }
}
