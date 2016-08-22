using System;
using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Hub.Managers;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using terminalFr8Core.Actions;
using Moq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Models;
using terminalFr8Core.Activities;

namespace terminalFr8CoreTests.Unit
{
    [TestFixture]
    [Category("Select_Fr8_Object_v1")]
    class SelectFr8Object_v1Tests : BaseTest
    {
        IDisposable _coreServer;
        Select_Fr8_Object_v1 select_Fr8_Object_v1;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();

            _coreServer = Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
            select_Fr8_Object_v1 = New<Select_Fr8_Object_v1>();
            Mock<IRestfulServiceClient> restClientMock = new Mock<IRestfulServiceClient>(MockBehavior.Default);
            restClientMock.Setup(restClient => restClient.GetAsync<CrateDTO>(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(Task.FromResult(FixtureData.TestEmptyCrateDTO()));
            ObjectFactory.Container.Inject(typeof(IRestfulServiceClient), restClientMock.Object);
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
        public async Task Evaluate_IsValidJSONResponse_For_InitialRequest()
        {
            ActivityContext activityContext = FixtureData.TestActivityContext1();
            await select_Fr8_Object_v1.Configure(activityContext);
            Assert.NotNull(activityContext);
            Assert.AreEqual(2, activityContext.ActivityPayload.CrateStorage.Count);
        }

        [Test]
        public async Task Evaluate_IsValidJSONResponse_For_FollowupRequest_PlanSelected()
        {
            ActivityContext activityContext = FixtureData.TestActivityContext1();
            await select_Fr8_Object_v1.Configure(activityContext);

            Assert.NotNull(activityContext);
           // Assert.AreEqual(2, actionDTO.CrateStorage.CrateDTO.Count);
        }

        [Test]
        public async Task Evaluate_IsValidJSONResponse_For_FollowupRequest_ContainerSelected()
        {
            ActivityContext activityContext = FixtureData.TestActivityContext1();
            await select_Fr8_Object_v1.Configure(activityContext);
            Assert.NotNull(activityContext);
            //Assert.AreEqual(2, actionDTO.CrateStorage.CrateDTO.Count);
        }
    }
}