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

namespace terminalBaseTests.BaseClasses
{
    [TestFixture]
    [Category("BaseTerminalController")]
    public class BaseTerminalControllerTests : BaseTest
    {
        IDisposable _coreServer;
        BaseTerminalController _baseTerminalController;
        string terminalName = "terminalDocuSign";
        ICrateManager CrateManagerHelper;
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
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
            await _baseTerminalController.HandleFr8Request(terminalName, "", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public async void HandleFr8Request_TerminalNotExist_ThrowsException()
        {
            ActionDTO actionDTO = new ActionDTO();
            actionDTO.ActivityTemplate = new ActivityTemplateDTO() { Name = "terminalDummy", Version = "1.1" };

            await _baseTerminalController.HandleFr8Request(terminalName, "", actionDTO);
        }

        [Test]
        public async void HandleFr8Request_Configure_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request("terminalActionMock", "configure", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActionDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActionDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "configure").Count(), 0);
        }

        [Test]
        public async void HandleFr8Request_Run_ReturnsPayloadDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request("terminalActionMock", "run", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async void HandleFr8Request_ChildrenExecuted_ReturnsPayloadDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request("terminalActionMock", "childrenexecuted", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(PayloadDTO), result);
        }

        [Test]
        public async void HandleFr8Request_InitialConfigurationResponse_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request("terminalActionMock", "initialconfigurationresponse", Fixture_HandleRequest.terminalMockActionDTO());

            var crateStorage = CrateManagerHelper.FromDto(((ActionDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "initialconfigurationresponse").Count(), 0);
        }

        [Test]
        public async void HandleFr8Request_FollowupConfigurationResponse_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request("terminalActionMock", "followupconfigurationresponse", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActionDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActionDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "followupconfigurationresponse").Count(), 0);
        }

        [Test]
        public async void HandleFr8Request_Activate_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request("terminalActionMock", "activate", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActionDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActionDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "activate").Count(), 0);
        }

        [Test]
        public async void HandleFr8Request_Othermethod_ReturnsActionDTO()
        {
            var result = await _baseTerminalController.HandleFr8Request("terminalActionMock", "OtherMethod", Fixture_HandleRequest.terminalMockActionDTO());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(ActionDTO), result);

            var crateStorage = CrateManagerHelper.FromDto(((ActionDTO)result).CrateStorage);
            var crateResult = crateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            Assert.Greater(crateResult.Controls.Where(x => x.Label.ToLower() == "othermethod").Count(), 0);
        }
    }
}
