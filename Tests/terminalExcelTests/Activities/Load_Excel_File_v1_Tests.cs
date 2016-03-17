using System;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalExcel.Actions;
using terminalExcelTests.Fixtures;
using TerminalBase.Infrastructure;
using UtilitiesTesting;

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
            ObjectFactory.Configure(x => x.For<IHubCommunicator>().Use(new Mock<IHubCommunicator>().Object));
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
    }
}
