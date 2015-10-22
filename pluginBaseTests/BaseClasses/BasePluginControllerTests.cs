﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using TerminalBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;

namespace pluginBaseTests.BaseClasses
{
    [TestFixture]
    [Category("BasePluginController")]
    public class BasePluginControllerTests : BaseTest
    {
        IDisposable _coreServer;
        BasePluginController _basePluginController;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginController = new BasePluginController();
            _coreServer = pluginBaseTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
        }

        [TearDown]
        public void TearDown()
        {
            if (_coreServer != null)
            {
                _coreServer.Dispose();
                _coreServer = null;
            }
        }

    }
}
