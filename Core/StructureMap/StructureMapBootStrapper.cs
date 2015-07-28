using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Repositories;
using Core.Interfaces;
using Core.ExternalServices;
using Core.ExternalServices.REST;
using Core.Managers;
using Core.Managers.APIManagers.Authorizers;
using Core.Managers.APIManagers.Authorizers.Google;
using Core.Managers.APIManagers.Packagers;
using Core.Managers.APIManagers.Packagers.SegmentIO;
using Core.Managers.APIManagers.Packagers.SendGrid;
using Core.Managers.APIManagers.Packagers.Twilio;
using Core.PluginRegistrations;
using Core.Security;
using Core.Services;
using Moq;
using SendGrid;
using StructureMap;
using StructureMap.Configuration.DSL;
using AutoMapper;
using Core.Managers.APIManagers.Authorizers.Docusign;
using Utilities;
using System;
using Core.Utilities;

namespace Core.StructureMap
{
    public class StructureMapBootStrapper
    {
        public enum DependencyType
        {
            TEST = 0,
            LIVE = 1
        }

        #region Method

        public static void ConfigureDependencies(DependencyType type)
        {

            switch (type)
            {
                case DependencyType.TEST:
                    ObjectFactory.Initialize(x => x.AddRegistry<TestMode>());
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                    break;
            }
        }

        public class CoreRegistry : Registry
        {
            public CoreRegistry()
            {
               
            }
        }

        public class LiveMode : DatabaseStructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<IConfigRepository>().Use<ConfigRepository>();
                For<ISMSPackager>().Use<TwilioPackager>();
                For<IMappingEngine>().Use(Mapper.Engine);
                //For<IEmailPackager>().Use<SendGridPackager>().Singleton().Named(EnvelopeDO.SendGridHander);
               
                For<IEmailAddress>().Use<EmailAddress>();
                For<INotification>().Use<Notification>();
              
                For<ISecurityServices>().Use<SecurityServices>();
                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<IOAuthAuthorizer>().Use<GoogleCalendarAuthorizer>().Named("Google");
                For<IOAuthAuthorizer>().Use<DocusignAuthorizer>().Named("Docusign");

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchy>();
                For<IImapClient>().Use<ImapClientWrapper>();
                For<ITransport>().Use(c => TransportFactory.CreateWeb(c.GetInstance<IConfigRepository>()));
                For<IRestfullCall>().Use<RestfulCallWrapper>();
                For<ITwilioRestClient>().Use<TwilioRestClientWrapper>();
                For<IProcessService>().Use<ProcessService>();
                For<IDocusignXml>().Use<DocusignXml>();
                For<ICriteria>().Use<Criteria>();

                For<IPluginRegistration>().Use<AzureSqlPluginRegistration>().Named("AzureSql");
            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<ISMSPackager>().Use<TwilioPackager>();
                For<IMappingEngine>().Use(Mapper.Engine);
                //For<IEmailPackager>().Use<SendGridPackager>().Singleton().Named(EnvelopeDO.SendGridHander);
               
                For<IEmailAddress>().Use<EmailAddress>();
                For<INotification>().Use<Notification>();
               
                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<ISecurityServices>().Use(new MockedSecurityServices());

                For<IOAuthAuthorizer>().Use<GoogleCalendarAuthorizer>().Named("Google");
                For<IOAuthAuthorizer>().Use<DocusignAuthorizer>().Named("Docusign");

                For<IRestfullCall>().Use<RestfulCallWrapper>();

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchyWithoutCTE>();
                var mockSegment = new Mock<ITracker>();
                For<ITracker>().Use(mockSegment.Object);
                For<IProcessService>().Use<ProcessService>();
                For<IDocusignXml>().Use<DocusignXml>();
                For<ICriteria>().Use<Criteria>();
                //var mockProcess = new Mock<IProcessService>();
                //mockProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
                //For<IProcessService>().Use(mockProcess.Object);
                //For<Mock<IProcessService>>().Use(mockProcess);

                For<IPluginRegistration>().Use<AzureSqlPluginRegistration>().Named("AzureSql");
            }
        }

        #endregion       
    }
}