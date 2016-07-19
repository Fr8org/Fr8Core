using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Services;
using NUnit.Framework;
using StructureMap;

using Fr8.Testing.Unit;

namespace HubTests.Services
{
    [TestFixture]
    [Category("Terminal")]
    public class TerminalServiceTests : BaseTest
    {
        private IApplicationSettings _originalSettings;
        private IConfigRepository _configRepository;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _originalSettings = CloudConfigurationManager.AppSettings;
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
        }

        [TearDown]
        public void Teardown()
        {
            CloudConfigurationManager.RegisterApplicationSettings(_originalSettings);
        }

        private bool AreEqual(TerminalDO a, TerminalDO b)
        {
            return a.AuthenticationType == b.AuthenticationType &&
                   a.Description == b.Description &&
                   a.Endpoint == b.Endpoint &&
                   a.Id == b.Id &&
                   a.Name == b.Name &&
                   a.Label == b.Label &&
                   a.Secret == b.Secret &&
                   a.TerminalStatus == b.TerminalStatus &&
                   a.Version == b.Version;
        }

        public IEnumerable<TerminalDO> GenerateTerminals(int count, string prefix = "")
        {
            for (int i = 1; i <= count; i ++)
            {
                yield return new TerminalDO
                {
                    Id = i,
                    AuthenticationType =1,
                    Endpoint = prefix+"ep" + i,
                    Description = prefix + "desc" + i,
                    Name = "name" + i,
                    Label = prefix + "Label" + i,
                    Version = prefix + "Ver" + i,
                    TerminalStatus = 1,
                };
            }
        }

        private void CompareCollections(TerminalDO[] reference, TerminalDO[] fact)
        {
            Assert.AreEqual(reference.Length, fact.Length);

            foreach (var t in fact)
            {
                bool found = false;

                foreach (var terminalDo in reference)
                {
                    if (AreEqual(t, terminalDo))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.IsTrue(found);
            }
        }

        private void ConfigureNoCache()
        {
            CloudConfigurationManager.RegisterApplicationSettings(new DisableCacheSettings(CloudConfigurationManager.AppSettings));
        }

        [Test]
        public void CanDisableCaching()
        {
            ConfigureNoCache();
            Assert.IsTrue(new Terminal(_configRepository).IsATandTCacheDisabled);
        }

        [Test]
        public void CanRunWithCaching()
        {
            Assert.IsFalse(new Terminal(_configRepository).IsATandTCacheDisabled);
        }

        [Test]
        public void CanRegisterTerminalsWithoutCache()
        {
            ConfigureNoCache();
            CanRegisterTerminals();
        }

        [Test]
        public void CanLoadCacheFromDbWithoutCache()
        {
            ConfigureNoCache();
            CanLoadCacheFromDb();
        }
        
        [Test]
        public void CanUpdateTerminalsWithoutCache()
        {
            ConfigureNoCache();
            CanUpdateTerminals();
        }

        [Test]
        public void CanIssueNewIdForTerminalsWithoutIdWithoutCache()
        {
            ConfigureNoCache();
            CanIssueNewIdForTerminalsWithoutId();
        }

        [Test]
        public void CanIssueNewIdForTerminalsWithoutId()
        {
            var terminalService = new Terminal(_configRepository);
            var t = GenerateTerminals(1).First();

            t.Id = 0;

            var terminal = terminalService.RegisterOrUpdate(t);

            Assert.IsTrue(terminal.Id > 0);
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminals = uow.TerminalRepository.GetAll().ToArray();
                Assert.AreEqual(terminal.Id, terminals[0].Id);
            }
        }

        [Test]
        public void CanIssueNewIdForNewTerminalsWithInvalidIdWithoutCache()
        {
            ConfigureNoCache();
            CanIssueNewIdForNewTerminalsWithInvalidId();
        }

        [Test]
        public void CanIssueNewIdForNewTerminalsWithInvalidId()
        {
            var terminalService = new Terminal(_configRepository);
            var t = GenerateTerminals(1).First();
            var terminal = terminalService.RegisterOrUpdate(t);

            var tNew = GenerateTerminals(10).Last();
            tNew.Id = t.Id;

            var newTerminal = terminalService.RegisterOrUpdate(tNew);

            Assert.IsTrue(terminal.Id != newTerminal.Id);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminals = uow.TerminalRepository.GetAll().ToArray();
                Assert.AreEqual(terminals.Length, 2);
                CompareCollections(terminals, new []{terminal, newTerminal});
            }
        }

        [Test]
        public void CanRegisterTerminals()
        {
            TerminalDO[] terminals;
            var terminalService = new Terminal(_configRepository);
            
            foreach (var terminal in GenerateTerminals(2))
            {
                terminalService.RegisterOrUpdate(terminal);
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminals = uow.TerminalRepository.GetAll().ToArray();
            }
           
            var terminalsFromService = terminalService.GetAll().ToArray();

            Assert.AreEqual(2, terminalsFromService.Length);

            CompareCollections(terminals, terminalsFromService);
        }
        
        [Test]
        public void CanLoadCacheFromDb()
        {
            TerminalDO[] terminals;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var terminal in GenerateTerminals(10))
                {
                    uow.TerminalRepository.Add(terminal);
                }

                uow.SaveChanges();

                terminals = uow.TerminalRepository.GetAll().ToArray();
            }

            var terminalService = new Terminal(_configRepository);
            var terminalsFromService = terminalService.GetAll().ToArray();

            Assert.AreEqual(10, terminalsFromService.Length);

            CompareCollections(terminals, terminalsFromService);
        }


        [Test]
        public void CanUpdateTerminals()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var terminal in GenerateTerminals(10))
                {
                    uow.TerminalRepository.Add(terminal);
                }

                uow.SaveChanges();
            }

            var terminalService = new Terminal(_configRepository);

            var reference = GenerateTerminals(10, "updated").ToArray();

            foreach (var terminal in reference)
            {
                terminalService.RegisterOrUpdate(terminal);
            }
            
            var terminalsFromService = terminalService.GetAll().ToArray();

            Assert.AreEqual(10, terminalsFromService.Length);
            CompareCollections(reference, terminalsFromService);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalsFromRepository = uow.TerminalRepository.GetAll().ToArray();

                Assert.AreEqual(10, terminalsFromRepository.Length);
                CompareCollections(reference, terminalsFromRepository);
            }
        }

    }
}
