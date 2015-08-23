using AutoMapper;
using Core.ExternalServices;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Authorizers;
using Core.Managers.APIManagers.Authorizers.Docusign;
using Core.Managers.APIManagers.Authorizers.Google;
using Core.Managers.APIManagers.Packagers;
using Core.Managers.APIManagers.Packagers.SegmentIO;
using Core.Managers.APIManagers.Packagers.SendGrid;
using Core.Managers.APIManagers.Packagers.Twilio;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.PluginRegistrations;
using Core.Security;
using Core.Services;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories;
using Data.Wrappers;
using Moq;
using SendGrid;
using StructureMap;
using StructureMap.Configuration.DSL;
using System.Threading.Tasks;
using Utilities;

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
                For<IEmailPackager>().Use<SendGridPackager>().Singleton().Named(MailerDO.SendGridHander);

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
                For<IRestfulServiceClient>().Use<RestfulServiceClient>();
                For<IPluginTransmitter>().Use<PluginTransmitter>();
                For<ITwilioRestClient>().Use<TwilioRestClientWrapper>();
                For<IProcessTemplate>().Use<ProcessTemplate>();
                For<IProcess>().Use<Process>();
                For<ICriteria>().Use<Criteria>();
                For<IAction>().Use<Action>();
                For<ISubscription>().Use<Subscription>();
                For<IProcessNode>().Use<ProcessNode>();
                For<IDocuSignNotification>().Use<DocuSignNotification>();
                For<IProcessNodeTemplate>().Use<ProcessNodeTemplate>();
                For<IPluginRegistration>().Use<AzureSqlServerPluginRegistration_v1>().Named("AzureSql");
                //For<IDocuSignTemplate>().Use<DocuSignTemplate>();
                For<IEvent>().Use<Event>();
                For<IEnvelope>().Use<DocuSignEnvelope>();
                For<IActionRegistration>().Use<ActionRegistration>();
                For<IDocuSignTemplate>().Use<DocuSignTemplate>();

            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<ISMSPackager>().Use<TwilioPackager>();
                For<IMappingEngine>().Use(Mapper.Engine);
                For<IEmailPackager>().Use<SendGridPackager>().Singleton().Named(MailerDO.SendGridHander);

                For<IEmailAddress>().Use<EmailAddress>();
                For<INotification>().Use<Notification>();

                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<ISecurityServices>().Use(new MockedSecurityServices());

                For<IOAuthAuthorizer>().Use<GoogleCalendarAuthorizer>().Named("Google");
                For<IOAuthAuthorizer>().Use<DocusignAuthorizer>().Named("Docusign");

                For<IRestfulServiceClient>().Use<RestfulServiceClient>();

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchyWithoutCTE>();
                var mockSegment = new Mock<ITracker>();
                For<IActionRegistration>().Use<ActionRegistration>();
                
                For<ITracker>().Use(mockSegment.Object);
                For<IProcess>().Use<Process>();
                For<ICriteria>().Use<Criteria>();
                For<ISubscription>().Use<Subscription>();
                For<IAction>().Use<Action>();
                For<IProcessNode>().Use<ProcessNode>();
                For<IDocuSignNotification>().Use<DocuSignNotification>();
                For<IProcessTemplate>().Use<ProcessTemplate>();
                For<IProcessNodeTemplate>().Use<ProcessNodeTemplate>();
                //var mockProcess = new Mock<IProcessService>();
                //mockProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
                //For<IProcessService>().Use(mockProcess.Object);
                //For<Mock<IProcessService>>().Use(mockProcess);

                var pluginTransmitterMock = new Mock<IPluginTransmitter>();
                pluginTransmitterMock.Setup(e => e.PostActionAsync(It.IsAny<string>(), It.IsAny<ActionPayloadDTO>())).Returns(Task.FromResult<string>("{\"success\": {\"ErrorCode\": \"0\", \"StatusCode\": \"200\", \"Description\": \"\"}}"));
                For<IPluginTransmitter>().Use(pluginTransmitterMock.Object).Singleton();
                For<IActionRegistration>().Use<ActionRegistration>();
                For<IPluginRegistration>().Use<AzureSqlServerPluginRegistration_v1>().Named("AzureSql");
                For<IEvent>().Use<Event>();
                For<IEnvelope>().Use<DocuSignEnvelope>();
                For<IDocuSignTemplate>().Use<DocuSignTemplate>();
            }
        }

        #endregion
    }
}