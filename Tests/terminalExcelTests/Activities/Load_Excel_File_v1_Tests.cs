using System;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.DataTransferObjects.Helpers;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalExcel.Actions;
using TerminalBase.Infrastructure;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace terminalExcelTests.Activities
{
    [TestFixture]
    [Category(nameof(Load_Excel_File_v1))]
    public class Load_Excel_File_v1_Tests : BaseTest
    {
        private ICrateManager _crateManager;
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _crateManager = new CrateManager();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            hubCommunicatorMock.Setup(x => x.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>()))
                               .Returns(Task.FromResult(FixtureData.PayloadDTO1()));
            ObjectFactory.Configure(x => x.For<IHubCommunicator>().Use(hubCommunicatorMock.Object));
        }

        [Test]
        [ExpectedException(typeof(AggregateException))]
        public void Configure_ThrowsExceptions_WhenActivityHasEmptyGuid()
        {
            var activity = new Load_Excel_File_v1();
            activity.Configure(new ActivityDO(), new AuthorizationTokenDO()).Wait();
        }

        [Test]
        public void Configure_WhenConfigurationIsInitial_HasOnlyControlsCrateInsideActivityStorage()
        {
            var activity = new Load_Excel_File_v1();
            var initialConfigurationResult = activity.Configure(new ActivityDO { Id = Guid.NewGuid() }, new AuthorizationTokenDO()).Result;
            var storage = _crateManager.GetStorage(initialConfigurationResult);
            Assert.AreEqual(1, storage.Count);
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>());
        }

        [Test]
        public void Configure_WhenConfigurationIsFollowup_HasRuntimeCratesDescriptionCrateInsideActivityStorage()
        {
            var activity = new Load_Excel_File_v1();
            var initialConfigResult = activity.Configure(new ActivityDO { Id = Guid.NewGuid() }, new AuthorizationTokenDO()).Result;
            var followupConfigResult = activity.Configure(initialConfigResult, new AuthorizationTokenDO()).Result;
            var storage = _crateManager.GetStorage(followupConfigResult);
            Assert.AreEqual(2, storage.Count);
            Assert.IsNotNull(storage.FirstCrateOrDefault<StandardConfigurationControlsCM>());
            Assert.IsNotNull(storage.FirstCrateOrDefault<CrateDescriptionCM>());
        }

        [Test]
        public void Run_WhenNoTableDataExistsInActivityStorage_ReturnsError()
        {
            var activity = new Load_Excel_File_v1();
            var initialConfigResult = activity.Configure(new ActivityDO { Id = Guid.NewGuid() }, new AuthorizationTokenDO()).Result;
            var result = activity.Run(initialConfigResult, Guid.Empty, new AuthorizationTokenDO()).Result;
            var operationState = _crateManager.GetOperationalState(result);
            ErrorDTO error;
            Assert.IsTrue(operationState.CurrentActivityResponse.TryParseErrorDTO(out error));
        }

        [Test]
        public void Run_WhenDataExists_HasTableDataInsidePayloadStorage()
        {
            var activity = new Load_Excel_File_v1();
            var initialConfigResult = activity.Configure(new ActivityDO { Id = Guid.NewGuid() }, new AuthorizationTokenDO()).Result;
            using (var activityStorage = _crateManager.GetUpdatableStorage(initialConfigResult))
            {
                activityStorage.Add(Crate.FromContent(string.Empty, new StandardTableDataCM(), AvailabilityType.RunTime));
            }
            var result = activity.Run(initialConfigResult, Guid.Empty, new AuthorizationTokenDO()).Result;
            var payloadStorage = _crateManager.GetStorage(result);
            Assert.IsNotNull(payloadStorage.FirstCrateOrDefault<StandardTableDataCM>());
        }
    }
}
