﻿using System;
using UtilitiesTesting;
using NUnit.Framework;
using TerminalBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using terminalTests.Fixtures;
using Data.Crates;
using Hub.Managers;
using Data.Interfaces.Manifests;
using System.Linq;
using terminalBaseTests.Actions;
using Hub.StructureMap;
using StructureMap;
using TerminalBase.Infrastructure;

namespace terminalBaseTests.BaseClasses
{
    [TestFixture]
    [Category("BaseTerminalController")]
    public class BaseTerminalControllerTests : BaseTest
    {
        IDisposable _coreServer;
        BaseTerminalController _baseTerminalController;
        string terminalName = "terminalBaseTests";
        ICrateManager CrateManagerHelper;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            ObjectFactory.Configure(cfg => cfg.For<IHubCommunicator>().Use<DefaultHubCommunicator>());
            CrateManagerHelper = new CrateManager();
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

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async void HandleFr8Request_NullActionDTO_ThrowsException()
        {
            await _baseTerminalController.HandleFr8Request(terminalName, "", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async void HandleFr8Request_NullActivityTemplate_ThrowsException()
        {
            var activityDTO = Fixture_HandleRequest.terminalMockActionDTO();
            activityDTO.ActivityTemplate = null;
            await _baseTerminalController.HandleFr8Request(terminalName, "", activityDTO);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async void HandleFr8Request_TerminalNotExist_ThrowsException()
        {
            ActivityDTO activityDTO = new ActivityDTO();
            activityDTO.ActivityTemplate = new ActivityTemplateDTO() { Name = "terminalDummy", Version = "1.1" };

            await _baseTerminalController.HandleFr8Request(terminalName, "", activityDTO);
        }

        [Test]
        public async void HandleFr8Request_Configure_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request(terminalName, "configure", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "configure").Count(), 0);
        }

        [Test]
        public async void HandleFr8Request_Run_ReturnsPayloadDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request(terminalName, "run", Fixture_HandleRequest.terminalMockActionDTOTest());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async void HandleFr8Request_ChildrenExecuted_ReturnsPayloadDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request(terminalName, "childrenexecuted", Fixture_HandleRequest.terminalMockActionDTOTest());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async void HandleFr8Request_Activate_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request(terminalName, "activate", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "activate").Count(), 0);
        }

        [Test]
        public async void HandleFr8Request_Deactivate_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request(terminalName, "deactivate", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "deactivate").Count(), 0);
        }

        [Test]
        public async void HandleFr8Request_Othermethod_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request(terminalName, "OtherMethod", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActivityDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActivityDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "othermethod").Count(), 0);
        }
    }
}
