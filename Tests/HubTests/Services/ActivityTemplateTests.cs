using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Interfaces;
using Hub.Services;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;

namespace HubTests.Services
{
    [TestFixture]
    [Category("ActivityTemplate")]
    public class ActivityTemplateTests : BaseTest
    {
        private IApplicationSettings _originalSettings;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _originalSettings = CloudConfigurationManager.AppSettings;
        }

        [TearDown]
        public void Teardown()
        {
            CloudConfigurationManager.RegisterApplicationSettings(_originalSettings);
        }

        private static bool AreEqual(ActivityTemplateDO a, ActivityTemplateDO b, bool skipId = false)
        {
            return a.NeedsAuthentication == b.NeedsAuthentication &&
                   a.ActivityTemplateState == b.ActivityTemplateState &&
                   a.Category == b.Category &&
                   a.ComponentActivities == b.ComponentActivities &&
                   a.Description == b.Description &&
                   (skipId || a.Id == b.Id) &&
                   a.Label == b.Label &&
                   a.MinPaneWidth == b.MinPaneWidth &&
                   a.Name == b.Name &&
                   a.Tags == b.Tags &&
                   (skipId || a.TerminalId == b.Terminal.Id) &&
                   AreEqual(a.Terminal, b.Terminal, skipId) &&
                   a.Type == b.Type &&
                   a.Version == b.Version &&
                   (skipId || a.WebServiceId == b.WebServiceId) &&
                   AreEqual(a.WebService, b.WebService, skipId);
        }

        private static bool AreEqual(WebServiceDO a, WebServiceDO b, bool skipId = false)
        {
            return a.IconPath == b.IconPath &&
                   (skipId || a.Id == b.Id) &&
                   a.Name == b.Name;
        }

        private static bool AreEqual(TerminalDO a, TerminalDO b, bool skipId = false)
        {
            return a.AuthenticationType == b.AuthenticationType &&
                   a.Description == b.Description &&
                   a.Endpoint == b.Endpoint &&
                   (skipId || a.Id == b.Id) &&
                   a.Name == b.Name &&
                   a.Label == b.Label &&
                   a.PublicIdentifier == b.PublicIdentifier &&
                   a.Secret == b.Secret &&
                   a.TerminalStatus == b.TerminalStatus &&
                   a.Version == b.Version;
        }


        private static void CheckIntegrity(ActivityTemplateDO activityTemplate)
        {
            Assert.IsNotNull(activityTemplate.Terminal);
            Assert.AreEqual(activityTemplate.Terminal.Id, activityTemplate.TerminalId);

            if (activityTemplate.WebServiceId != null)
            {
                Assert.IsNotNull(activityTemplate.WebService);
                Assert.AreEqual(activityTemplate.WebServiceId.Value, activityTemplate.WebService.Id);
            }
            else
            {
                Assert.IsNull(activityTemplate.WebService);
            }

            AreEqual(activityTemplate.Terminal, ObjectFactory.GetInstance<ITerminal>().GetByKey(activityTemplate.TerminalId));

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (activityTemplate.WebServiceId != null)
                {
                    AreEqual(activityTemplate.WebService, uow.WebServiceRepository.GetByKey(activityTemplate.WebServiceId));
                }
            }
        }

        public IEnumerable<TerminalDO> GenerateTerminals(int count, string prefix = "")
        {
            for (int id = 1; id <= count; id++)
            {
                yield return CreateTerminal(id, prefix);
            }
        }

        private TerminalDO CreateTerminal(int id, string prefix = "")
        {
            return new TerminalDO
            {
                Id = id,
                AuthenticationType = 1,
                Endpoint = prefix + "ep" + id,
                Description = prefix + "desc" + id,
                Name = "name" + id,
                Label = prefix + "Label" + id,
                Version = prefix + "Ver" + id,
                TerminalStatus = 1,
            };
        }

        public IEnumerable<WebServiceDO> GenerateWebServices(int count, string prefix = "")
        {
            for (int i = 1; i <= count; i++)
            {
                yield return CreateWebService(i, prefix);
            }
        }

        private WebServiceDO CreateWebService(int id, string prefix = "")
        {
            return new WebServiceDO()
            {
                Id = id,
                Name = "name" + id,
                IconPath = prefix + "iconPath" + id
            };
        }

        private ActivityTemplateDO CreateActivityTemplate(Guid id, TerminalDO terminal, WebServiceDO webService, string prefix = "")
        {
            return new ActivityTemplateDO
            {
                Id = id,
                ActivityTemplateState = 1,
                Category = ActivityCategory.Forwarders,
                MinPaneWidth = 330,
                ComponentActivities = prefix + "ca" + id,
                Description = prefix + "des" + id,
                Name = "name" + id,
                Label = prefix + "label" + id,
                NeedsAuthentication = true,
                Tags = prefix + "tags" + id,
                TerminalId = terminal.Id,
                Terminal = terminal,
                Type = ActivityType.Standard,
                Version = "1",
                WebService = webService,
                WebServiceId = webService?.Id
            };
        }

        private void CompareCollections(ActivityTemplateDO[] reference, ActivityTemplateDO[] fact, bool skipIds = false)
        {
            Assert.AreEqual(reference.Length, fact.Length);

            foreach (var t in fact)
            {
                bool found = false;

                foreach (var activityTemplateDo in reference)
                {
                    if (AreEqual(t, activityTemplateDo, skipIds))
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
            Assert.IsTrue(ObjectFactory.GetInstance<ActivityTemplate>().IsATandTCacheDisabled);
        }

        [Test]
        public void CanRunWithCaching()
        {
            Assert.IsFalse(ObjectFactory.GetInstance<ActivityTemplate>().IsATandTCacheDisabled);
        }

        private ActivityTemplateDO[] GenerateSeedData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminals = GenerateTerminals(5).ToArray();

                foreach (var terminal in terminals)
                {
                    uow.TerminalRepository.Add(terminal);
                }

                var webServices = new WebServiceDO[5];

                for (int i = 1; i <= 5; i++)
                {
                    webServices[i - 1] = CreateWebService(i);
                    uow.WebServiceRepository.Add(webServices[i - 1]);
                }

                uow.SaveChanges();

                var templates = new ActivityTemplateDO[20];

                for (int i = 1; i <= 20; i++)
                {
                    templates[i - 1] = CreateActivityTemplate(FixtureData.GetTestGuidById(i), terminals[i % 5], webServices[i % 5]);
                    uow.ActivityTemplateRepository.Add(templates[i - 1]);
                }

                uow.SaveChanges();

                return templates;
            }
        }

        [Test]
        public void CanLoadCacheFromDbWithoutCache()
        {
            ConfigureNoCache();
            CanLoadCacheFromDb();
        }

        [Test]
        public void CanRegisterWithoutCache()
        {
            ConfigureNoCache();
            CanRegister();
        }

        [Test]
        public void CanRegister()
        {
            var template = CreateActivityTemplate(Guid.NewGuid(), CreateTerminal(-234, "new"), CreateWebService(234234, "new"));
            template.WebServiceId = -2344;

            var terminalService = ObjectFactory.GetInstance<Terminal>();

            template.Terminal = terminalService.RegisterOrUpdate(template.Terminal);
            template.TerminalId = template.Terminal.Id;

            var service = ObjectFactory.GetInstance<ActivityTemplate>();
            service.RegisterOrUpdate(template);

            CheckIfInSyncWithDd(service, 1);
            AreEqual(template, service.GetAll().First(), true);

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                AreEqual(CreateWebService(234234, "new"), uow.WebServiceRepository.GetQuery().Single(), true);
            }
        }

        [Test]
        public void CanLoadCacheFromDb()
        {
            ActivityTemplateDO[] templates;

            GenerateSeedData();
            CheckIfInSyncWithDd(ObjectFactory.GetInstance<ActivityTemplate>(), 20);
        }

        [Test]
        public void CanUpdate()
        {
            GenerateSeedData();

            var template = CreateActivityTemplate(FixtureData.GetTestGuidById(1), CreateTerminal(-234), CreateWebService(234234));

            template.WebServiceId = -2344;
            template.Id = Guid.Empty;

            var terminalService = ObjectFactory.GetInstance<Terminal>();
            template.Terminal = terminalService.RegisterOrUpdate(template.Terminal);
            template.TerminalId = template.Terminal.Id;

            var service = ObjectFactory.GetInstance<ActivityTemplate>();

            service.RegisterOrUpdate(template);

            var storedTemplate = service.GetQuery().First(x => x.Name == template.Name);

            CheckIntegrity(storedTemplate);

            AreEqual(template, storedTemplate, true);

            CheckIfInSyncWithDd(service, 21);
        }

        private void CheckIfInSyncWithDd(ActivityTemplate activityTemplateService, int count)
        {
            ActivityTemplateDO[] templates;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                templates = uow.ActivityTemplateRepository.GetAll().ToArray();
            }

            var templatesFromServices = activityTemplateService.GetAll();

            foreach (var templatesFromService in templatesFromServices)
            {
                CheckIntegrity(templatesFromService);
            }

            Assert.AreEqual(count, templatesFromServices.Length);

            CompareCollections(templates, templatesFromServices);
        }
    }
}
