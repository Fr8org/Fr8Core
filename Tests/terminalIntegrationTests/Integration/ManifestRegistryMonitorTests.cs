using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Testing.Integration;
using Hub.Interfaces;
using Hub.Services;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace terminalIntegrationTests.Integration
{
    [TestFixture]
    [Explicit]
    public class ManifestRegistryMonitorTests : BaseHubIntegrationTest
    {
        public override string TerminalName => "terminalGoogle";

        private IConfigRepository _originalConfigRepository;
        private IFr8Account _fr8Account;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = ObjectFactory.Container.GetNestedContainer();
            _container.Inject(new Mock<IPusherNotifier>().Object);
        }

        private ManifestRegistryMonitor CreateTarget(string systemUserEmail = null)
        {
            var configRepositoryMock = new Mock<IConfigRepository>();
                configRepositoryMock.Setup(x => x.Get("SystemUserEmail"))
                                    .Returns(systemUserEmail);
            _container.Inject(configRepositoryMock.Object);

            var fr8AccountMock = new Mock<IFr8Account>();
            fr8AccountMock.Setup(x => x.GetSystemUser())
                    .Returns(() => null);
            _container.Inject(fr8AccountMock.Object);

            var target = _container.GetInstance<ManifestRegistryMonitor>();
            return target;
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "System user doesn't exist")]
        public async Task ManifestRegistryMonitor_WhenNoSystemUserEmailIsStored_ThrowsException()
        {
            var target = CreateTarget();
            await target.StartMonitoringManifestRegistrySubmissions();
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "System user doesn't exist")]
        public async Task ManifestRegistryMonitor_WhenNoSystemUserExists_ThrowsException()
        {
            var target = CreateTarget("fake@fake.com");
            await target.StartMonitoringManifestRegistrySubmissions();
        }
    }
}
