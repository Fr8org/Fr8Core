using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using StructureMap;
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
        private ICrateManager _crateManager;

        [SetUp]
        public override void SetUp()
        {
           

            base.SetUp();

            CloudConfigurationManager.RegisterApplicationSettings(new AppSettingsFixture());

            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _basePluginController = new BasePluginController();
        }

        [Test]
        public async void HandleDockyardRequest_PluginTypeIsAzureSqlServer_ResponseInitialConfiguration()
        {
            string curPlugin = "terminalAzure";
            string curActionPath = "Configure";

            ActionDTO curActionDTO = FixtureData.TestActionDTO1();

            var result = await (Task<ActionDTO>)_basePluginController.HandleFr8Request(curPlugin, curActionPath, curActionDTO);

            Assert.AreEqual("Standard Configuration Controls", _crateManager.FromDto(result.CrateStorage).First().ManifestType.Type);
        }
    }
}