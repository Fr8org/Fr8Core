﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using TerminalBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;

namespace terminalBaseTests.BaseClasses
{
    [TestFixture]
    [Category("BaseTerminalController")]
    public class BaseTerminalControllerTests : BaseTest
    {
        IDisposable _coreServer;
        BaseTerminalController _baseTerminalController;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _baseTerminalController = new BaseTerminalController();
            _coreServer = terminalBaseTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
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
