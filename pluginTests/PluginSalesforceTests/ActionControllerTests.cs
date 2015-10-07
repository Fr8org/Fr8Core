﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using terminal_base.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using terminal_Salesforce;

namespace terminalTests.PluginSalesforceTests
{
    [TestFixture]
    public class SalesforceActionControllerTests : BaseTest
    {
        BaseTerminalController _baseTerminalController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _baseTerminalController = new BaseTerminalController();
            TerminalSalesforceStructureMapBootstrapper.ConfigureDependencies(TerminalSalesforceStructureMapBootstrapper.DependencyType.LIVE);            
        }

        [Test]
        public void HandleDockyardRequest_PluginTypeIsSalesforce_CreateLead()
        {
            string curPlugin = "terminal_Salesforce";
            string curActionPath = "CreateLead";
            ActionDTO curActionDTO = FixtureData.TestActionDTOForSalesforce();            
            ActionDTO result = (ActionDTO)_baseTerminalController.HandleDockyardRequest(curPlugin, curActionPath, curActionDTO);
            Assert.AreEqual(curActionDTO, result);
        }
    }
}
