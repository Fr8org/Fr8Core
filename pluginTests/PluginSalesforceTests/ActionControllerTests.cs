﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using terminalBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using pluginSalesforce;

namespace terminalTests.PluginSalesforceTests
{
    [TestFixture]
    public class SalesforceActionControllerTests : BaseTest
    {
        BaseTerminalController _basePluginController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginController = new BaseTerminalController();
            PluginSalesforceStructureMapBootstrapper.ConfigureDependencies(PluginSalesforceStructureMapBootstrapper.DependencyType.LIVE);            
        }

        [Test]
        public void HandleDockyardRequest_PluginTypeIsSalesforce_CreateLead()
        {
            string curPlugin = "pluginSalesforce";
            string curActionPath = "CreateLead";
            ActionDTO curActionDTO = FixtureData.TestActionDTOForSalesforce();            
            ActionDTO result = (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, curActionPath, curActionDTO);
            Assert.AreEqual(curActionDTO, result);
        }
    }
}
