using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using terminalAzure.Controllers;
using UtilitiesTesting;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using PluginBase.BaseClasses;

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
            _basePluginController = new BasePluginController();
        }

        [Test]
        public async void HandleDockyardRequest_PluginTypeIsAzureSqlServer_ResponseInitialConfiguration()
        {
            string curPlugin = "pluginAzureSqlServer";
            string curActionPath = "Configure";

            ActionDTO curActionDTO = FixtureData.TestActionDTO1();

            ActionDTO actionDTO = await (Task<ActionDTO>)_basePluginController
                .HandleDockyardRequest(curPlugin, curActionPath, curActionDTO);

            Assert.AreEqual("Standard Configuration Controls", actionDTO.CrateStorage.CrateDTO[0].ManifestType);
        }
    }
}