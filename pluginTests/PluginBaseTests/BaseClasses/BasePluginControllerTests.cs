﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using terminal_base.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;

namespace terminalTests.TerminalBaseTests.Controllers
{
    [TestFixture]
    [Category("BasePluginController")]
    public class BasePluginControllerTests : BaseTest
    {
        IDisposable _coreServer;
        BaseTerminalController _basePluginController;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginController = new BaseTerminalController();
            _coreServer = FixtureData.CreateCoreServer_ActivitiesController();
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

        [Test]
        public async void HandleDockyardRequest_PluginTypeIsAzureSqlServer_ResponseInitialConfiguration()
        {
            string curPlugin = "pluginAzureSqlServer";
            string curActionPath = "Configure";

            ActionDTO curActionDTO = FixtureData.TestActionDTO1();

            ActionDTO actionDTO = await (Task<ActionDTO>) _basePluginController
                .HandleDockyardRequest(curPlugin, curActionPath, curActionDTO);

            Assert.AreEqual("Standard Configuration Controls", actionDTO.CrateStorage.CrateDTO[0].ManifestType);
        }

       



    }
}
