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
    }
}
