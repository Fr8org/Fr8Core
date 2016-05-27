using System.Linq;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using NUnit.Framework;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using terminalAzure.Tests.Fixtures;

namespace terminalAzure.Tests.Controllers
{
    [TestFixture]
    public class ActivityControllerTest : BaseTest
    {
        BaseTerminalController _baseTerminalController;
        private ICrateManager _crateManager;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();

            CloudConfigurationManager.RegisterApplicationSettings(new AppSettingsFixture());
            
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _baseTerminalController = new BaseTerminalController();
        }

        [Test]
        public async Task HandleDockyardRequest_TerminalTypeIsAzureSqlServer_ResponseInitialConfiguration()
        {
            string curTerminal = "terminalAzure";
            string curActionPath = "Configure";

            ActivityDTO curActionDTO = FixtureData.TestActivityDTO1();
            var fr8Data = new Fr8DataDTO { ActivityDTO = curActionDTO };
            object response = await _baseTerminalController.HandleFr8Request(curTerminal, curActionPath, fr8Data);
            ActivityDTO activityDTO = (ActivityDTO) response;
            Assert.AreEqual("Standard UI Controls", _crateManager.FromDto(activityDTO.CrateStorage).First().ManifestType.Type);
        }
    }
}