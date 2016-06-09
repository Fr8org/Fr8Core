using System;
using System.Collections.Generic;
using UtilitiesTesting;
using NUnit.Framework;
using System.Threading.Tasks;
using terminalTests.Fixtures;
using System.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Hub.StructureMap;
using StructureMap;
using Moq;
using terminaBaselTests.BaseClasses;

namespace terminalBaseTests.BaseClasses
{
    [TestFixture]
    [Category("ActivityExecutor")]
    public class ActivityExecutorTests : BaseTest
    {
        ActivityExecutor _activityExecutor;
        string terminalName = "terminalBaseTests";
        ICrateManager CrateManagerHelper;
        private IActivityStore _activityStore;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            AutoMapperBootstrapper.ConfigureAutoMapper();
            ObjectFactory.Configure(x => x.AddRegistry<StructureMapBootStrapper.TestMode>());
            ObjectFactory.Configure(x => x.For<IActivityStore>().Use<ActivityStore>().Singleton());
            
            var crateStorage = new CrateStorage(Crate.FromContent("", new OperationalStateCM()));
            var crateDTO = CrateManager.ToDto(crateStorage);
            var hubCommunicatorMock = new Mock<IHubCommunicator>();

            hubCommunicatorMock.Setup(x => x.GetPayload(It.IsAny<Guid>()))
                .ReturnsAsync(new PayloadDTO(Guid.NewGuid())
                {
                     CrateStorage = crateDTO
                });

            ObjectFactory.Configure(cfg => cfg.For<IHubCommunicator>().Use(hubCommunicatorMock.Object));

            CrateManagerHelper = new CrateManager();
            _activityExecutor = ObjectFactory.GetInstance<ActivityExecutor>();
            _activityStore = ObjectFactory.GetInstance<IActivityStore>();

            if (_activityStore.GetValue(BaseTerminalActivityMock.ActivityTemplate) == null)
            {
                _activityStore.RegisterActivity<BaseTerminalActivityMock>(BaseTerminalActivityMock.ActivityTemplate);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task HandleFr8Request_NullActivityDTO_ThrowsException()
        {
            await _activityExecutor.HandleFr8Request(terminalName, "", null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async Task HandleFr8Request_NullActivityTemplate_ThrowsException()
        {
            var activityDTO = Fixture_HandleRequest.terminalMockActivityDTO();
            activityDTO.ActivityTemplate = null;
            var fr8Data = new Fr8DataDTO { ActivityDTO = activityDTO };
            await _activityExecutor.HandleFr8Request(terminalName, "", null, fr8Data);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async Task HandleFr8Request_TerminalNotExist_ThrowsException()
        {
            ActivityDTO activityDTO = new ActivityDTO();
            activityDTO.ActivityTemplate = new ActivityTemplateDTO() { Name = "terminalDummy", Version = "1.1" };
            var fr8Data = new Fr8DataDTO { ActivityDTO = activityDTO };
            await _activityExecutor.HandleFr8Request(terminalName, "", null, fr8Data);
        }

        [Test]
        public async Task HandleFr8Request_Configure_ReturnsActivityDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(
                terminalName,
                "configure",
                null,
                Fixture_HandleRequest.terminalMockFr8DataDTO()
            );

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            Assert.NotNull(crateStorage);
            //var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
            //Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "configure"), 0);
        }

        [Test]
        public async Task HandleFr8Request_Run_ReturnsPayloadDTO()
        {
            var f8Data = Fixture_HandleRequest.terminalMockFr8DataDTO();
            f8Data.ContainerId = Guid.NewGuid();
            var result = await _activityExecutor.HandleFr8Request(terminalName, "run", null, f8Data);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async Task HandleFr8Request_ExecuteChildActivities_ReturnsPayloadDTO()
        {
            var f8Data = Fixture_HandleRequest.terminalMockFr8DataDTO();
            f8Data.ContainerId = Guid.NewGuid();

            var parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("scope", "childActivities")
            };

            var result = await _activityExecutor.HandleFr8Request(terminalName, "run", parameters, f8Data);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async Task HandleFr8Request_Activate_ReturnsActivityDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "activate", null, Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            Assert.NotNull(crateStorage);
            //var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
            //Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "activate"), 0);
        }

        [Test]
        public async Task HandleFr8Request_Deactivate_ReturnsActivityDTO()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "deactivate", null, Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            Assert.NotNull(crateStorage);
            //var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();
            //Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "deactivate"), 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async Task HandleFr8Request_Othermethod_ShouldThrowException()
        {
            var result = await _activityExecutor.HandleFr8Request(terminalName, "OtherMethod", null, Fixture_HandleRequest.terminalMockFr8DataDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Count(x => x.Label.ToLower() == "othermethod"), 0);
        }
    }
}
