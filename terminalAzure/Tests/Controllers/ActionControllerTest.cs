using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using Utilities.Configuration.Azure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using terminalAzure.Controllers;
using terminalAzure.Tests.Fixtures;

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

            ActionDTO actionDTO = await (Task<ActionDTO>)_basePluginController
                .HandleDockyardRequest(curPlugin, curActionPath, curActionDTO);

            Assert.AreEqual("Standard Configuration Controls", actionDTO.CrateStorage.CrateDTO[0].ManifestType);
        }
    }
}