using System;
using UtilitiesTesting;
using NUnit.Framework;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using terminalTests.Fixtures;
using Hub.Managers;
using System.Linq;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.StructureMap;
using StructureMap;
using TerminalBase.Infrastructure;
using Fr8Data.Managers;
using terminaBaselTests.BaseClasses;
using TerminalBase.Services;

namespace terminalBaseTests.BaseClasses
{
    [TestFixture]
    [Category("ActivityExecutor")]
    public class ActivityExecutorTests : BaseTest
    {
        IDisposable _coreServer;
        ActivityExecutor _activityExecutor;
        string terminalName = "terminalBaseTests";
        ICrateManager CrateManagerHelper;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            ObjectFactory.Configure(cfg => cfg.For<IHubCommunicator>().Use<DefaultHubCommunicator>());
            CrateManagerHelper = new CrateManager();
            _activityExecutor = ObjectFactory.GetInstance<ActivityExecutor>();
            _coreServer = terminalBaseTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
            ActivityStore.RegisterActivity<BaseTerminalActivityMock>(BaseTerminalActivityMock.ActivityTemplate);
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
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task HandleFr8Request_NullActivityDTO_ThrowsException()
        {
            await _activityExecutor.HandleFr8Request(terminalName, "", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async Task HandleFr8Request_NullActivityTemplate_ThrowsException()
        {
            var activityDTO = Fixture_HandleRequest.terminalMockActivityDTO();
            activityDTO.ActivityTemplate = null;
            var fr8Data = new Fr8DataDTO { ActivityDTO = activityDTO };
            await _activityExecutor.HandleFr8Request(terminalName, "", fr8Data);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async Task HandleFr8Request_TerminalNotExist_ThrowsException()
        {
            ActivityDTO activityDTO = new ActivityDTO();
            activityDTO.ActivityTemplate = new ActivityTemplateDTO() { Name = "terminalDummy", Version = "1.1" };
            var fr8Data = new Fr8DataDTO { ActivityDTO = activityDTO };
            await _activityExecutor.HandleFr8Request(terminalName, "", fr8Data);
        }

        [Test]
        public async Task HandleFr8Request_Configure_ReturnsActivityDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "configure", Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "configure"), 0);
        }

        [Test]
        public async Task HandleFr8Request_Run_ReturnsPayloadDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "run", Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async Task HandleFr8Request_ExecuteChildActivities_ReturnsPayloadDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "executechildactivities", Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async Task HandleFr8Request_Activate_ReturnsActivityDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "activate", Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "activate"), 0);
        }

        [Test]
        public async Task HandleFr8Request_Deactivate_ReturnsActivityDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "deactivate", Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "deactivate"), 0);
        }

        [Test]
        public async Task HandleFr8Request_Othermethod_ReturnsActivityDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "OtherMethod", Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "othermethod"), 0);
        }
    }
}
