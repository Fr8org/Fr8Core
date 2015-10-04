using System.Net.Http.Formatting;
using AutoMapper;
using Core.ExternalServices;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Authorizers;
using Core.Managers.APIManagers.Authorizers.Docusign;
using Core.Managers.APIManagers.Authorizers.Google;
using Core.Managers.APIManagers.Packagers;
using Core.Managers.APIManagers.Packagers.SegmentIO;
using Core.Managers.APIManagers.Packagers.Twilio;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.Security;
using Core.Services;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories;
using DocuSign.Integrations.Client;
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

        public static IContainer ConfigureDependencies(DependencyType type)
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
            return ObjectFactory.Container;
        }

        public static void LiveConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
        }

        public static void TestConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<LiveMode>();
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

                For<IEmailAddress>().Use<EmailAddress>();
                For<INotification>().Use<Core.Services.Notification>();

                For<ISecurityServices>().Use<SecurityServices>();
                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<IOAuthAuthorizer>().Use<GoogleAuthorizer>().Named("Google");
                For<IOAuthAuthorizer>().Use<DocusignAuthorizer>().Named("Docusign");

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchy>();
                For<IImapClient>().Use<ImapClientWrapper>();
                
                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();
                For<IRestfulServiceClient>().Use<RestfulServiceClient>();
                For<IPluginTransmitter>().Use<PluginTransmitter>();
                For<ITwilioRestClient>().Use<TwilioRestClientWrapper>();
                For<IProcessTemplate>().Use<ProcessTemplate>();
                For<IProcess>().Use<Process>();
                For<ICriteria>().Use<Criteria>();
                For<IAction>().Use<Action>();
				For<IActivity>().Use<Activity>();
                For<ISubscription>().Use<Subscription>();
                For<IProcessNode>().Use<ProcessNode>();
                For<IProcessNodeTemplate>().Use<ProcessNodeTemplate>();
                //For<IDocuSignTemplate>().Use<DocuSignTemplate>();
                For<IEvent>().Use<Event>();
                For<IActivityTemplate>().Use<ActivityTemplate>();
                For<IActionList>().Use<ActionList>();
                For<IFile>().Use<File>();
                For<ISMSMessage>().Use<SMSMessage>();
                For<IPlugin>().Use<Plugin>();
                For<ICrate>().Use<Crate>();
                For<IDockyardEvent>().Use<DockyardEvent>();
                For<IReport>().Use<Report>();
            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
              
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<ISMSPackager>().Use<TwilioPackager>();
                For<IMappingEngine>().Use(Mapper.Engine);

                For<IEmailAddress>().Use<EmailAddress>();
                For<INotification>().Use<Core.Services.Notification>();

                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<ISecurityServices>().Use(new MockedSecurityServices());

                For<IOAuthAuthorizer>().Use<GoogleAuthorizer>().Named("Google");
                For<IOAuthAuthorizer>().Use<DocusignAuthorizer>().Named("Docusign");

                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();

                Mock<RestfulServiceClient> restfulServiceClientMock = new Mock<RestfulServiceClient>(MockBehavior.Default);
                For<IRestfulServiceClient>().Use(restfulServiceClientMock.Object).Singleton();

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchyWithoutCTE>();
                var mockSegment = new Mock<ITracker>();
                For<IActivityTemplate>().Use<ActivityTemplate>();
                
                For<ITracker>().Use(mockSegment.Object);
                For<IProcess>().Use<Process>();
                For<ICriteria>().Use<Criteria>();
                For<ISubscription>().Use<Subscription>();
                For<IAction>().Use<Action>();
					 For<IActivity>().Use<Activity>();

                For<IProcessNode>().Use<ProcessNode>();
                For<IProcessTemplate>().Use<ProcessTemplate>();
                For<IProcessNodeTemplate>().Use<ProcessNodeTemplate>();
                //var mockProcess = new Mock<IProcessService>();
                //mockProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
                //For<IProcessService>().Use(mockProcess.Object);
                //For<Mock<IProcessService>>().Use(mockProcess);

                var pluginTransmitterMock = new Mock<IPluginTransmitter>();
                For<IPluginTransmitter>().Use(pluginTransmitterMock.Object).Singleton();
                For<IActivityTemplate>().Use<ActivityTemplate>();
                For<IEvent>().Use<Event>();
                //For<ITemplate>().Use<Services.Template>();
                For<IActionList>().Use<ActionList>();
                For<IFile>().Use<File>();
                For<ISMSMessage>().Use<SMSMessage>();
                For<IPlugin>().Use<Plugin>();
                For<ICrate>().Use<Crate>();
                For<IDockyardEvent>().Use<DockyardEvent>();
            }
        }

        #endregion
    }
}