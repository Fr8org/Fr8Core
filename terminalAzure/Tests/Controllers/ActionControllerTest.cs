using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using Utilities.Configuration.Azure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using terminalAzure.Controllers;
using terminalAzure.Tests.Fixtures;
using AutoMapper;
using Data.Entities;

namespace terminalAzure.Tests.Controllers
{
    [TestFixture]
    public class ActionControllerTest : BaseTest
    {
        BasePluginController _basePluginController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            CloudConfigurationManager.RegisterApplicationSettings(new AppSettingsFixture());

            _basePluginController = new BasePluginController();
        }

        [Test]
        public async void HandleDockyardRequest_PluginTypeIsAzureSqlServer_ResponseInitialConfiguration()
        {
            string curPlugin = "terminalAzure";
            string curActionPath = "Configure";

            ActionDTO curActionDTO = FixtureData.TestActionDTO1();

            var result = await (Task<ActionDTO>)_basePluginController.HandleFr8Request(curPlugin, curActionPath, curActionDTO);

            Assert.AreEqual("Standard Configuration Controls", result.CrateStorage.CrateDTO[0].ManifestType);
        }
    }
}