﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using PluginBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using pluginSalesforce;

namespace pluginTests.PluginSalesforceTests
{
    [TestFixture]
    public class SalesforceActionControllerTests : BaseTest
    {
        BasePluginController _basePluginController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginController = new BasePluginController();
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
