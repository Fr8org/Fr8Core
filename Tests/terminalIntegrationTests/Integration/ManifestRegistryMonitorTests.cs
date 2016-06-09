using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Testing.Integration;
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

        [SetUp]
        public void SetUp()
        {
            ObjectFactory.Container.Inject(new Mock<IPusherNotifier>().Object);
            if (_originalConfigRepository == null)
            {
                _originalConfigRepository = ObjectFactory.GetInstance<IConfigRepository>();
            }
        }

        private ManifestRegistryMonitor CreateTarget(string systemUserEmail = null)
        {
            var target = ObjectFactory.GetInstance<ManifestRegistryMonitor>();
            var configRepositoryMock = new Mock<IConfigRepository>();
            if (systemUserEmail != null)
            {
                configRepositoryMock.Setup(x => x.Get("SystemUserEmail"))
                                    .Returns(systemUserEmail);
            }
            ObjectFactory.Container.Inject(configRepositoryMock.Object);
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
