using System.Net.Http.Formatting;
using AutoMapper;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using Hub.Managers.APIManagers.Packagers.SendGrid;
using InternalInterfaces = Hub.Interfaces;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Authorizers;
using Hub.Managers.APIManagers.Authorizers.Docusign;
using Hub.Managers.APIManagers.Authorizers.Google;
using Hub.Managers.APIManagers.Packagers;
using Hub.Managers.APIManagers.Packagers.SegmentIO;
using Hub.Managers.APIManagers.Transmitters.Terminal;
using Hub.Managers.APIManagers.Transmitters.Restful;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using InternalClass = Hub.Services;
using Hub.Services;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories;
using DocuSign.Integrations.Client;
using Hub.ExternalServices;
using Hub.Security;
using Moq;
using SendGrid;
// This is used to avoid ambiguity between StructureMap.IContainer and  Core.Interfaces.IContainer
using ExtternalStructureMap = StructureMap;
using StructureMap;
using StructureMap.Configuration.DSL;
using System.Threading.Tasks;
using Utilities;
using Utilities.Interfaces;


namespace Hub.StructureMap
{
    public class StructureMapBootStrapper
    {
        public enum DependencyType
        {
            TEST = 0,
            LIVE = 1
        }

        #region Method

        public static ExtternalStructureMap.IContainer ConfigureDependencies(DependencyType type)
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
                For<IMappingEngine>().Use(Mapper.Engine);

                For<IEmailAddress>().Use<EmailAddress>();
                For<IEmailPackager>().Use<SendGridPackager>();
                For<INotification>().Use<Hub.Services.Notification>();

                For<ISecurityServices>().Use<SecurityServices>();
                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<IOAuthAuthorizer>().Use<GoogleAuthorizer>().Named("Google");
                For<IOAuthAuthorizer>().Use<DocusignAuthorizer>().Named("Docusign");

                For<IProfileNodeHierarchy>().Use<ProfileNodeHierarchy>();
                For<IImapClient>().Use<ImapClientWrapper>();
                
                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();
                For<IRestfulServiceClient>().Singleton().Use<RestfulServiceClient>();
                For<ITerminalTransmitter>().Use<TerminalTransmitter>();
                For<IRoute>().Use<Route>();
                For<InternalInterfaces.IContainer>().Use<InternalClass.Container>();
                For<ICriteria>().Use<Criteria>();
                For<IAction>().Use<Action>();
				For<IRouteNode>().Use<RouteNode>();
                For<ISubscription>().Use<Subscription>();
                For<IProcessNode>().Use<ProcessNode>();
                For<ISubroute>().Use<Subroute>();
                For<IField>().Use<Field>();
                //For<IDocuSignTemplate>().Use<DocuSignTemplate>();
                For<IEvent>().Use<Event>();
                For<IActivityTemplate>().Use<ActivityTemplate>();
                For<IFile>().Use<File>();
                For<ITerminal>().Use<Terminal>();
                For<ICrateManager>().Use<CrateManager>();
                For<IFr8Event>().Use<Fr8Event>();
                For<IReport>().Use<Report>();
                For<IManifest>().Use<Manifest>();
                For<IFindObjectsRoute>().Use<FindObjectsRoute>();
	            For<ITime>().Use<Time>();
	            For<IPusherNotifier>().Use<PusherNotifier>();
                For<IAuthorization>().Use<Authorization>();
            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
              
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<IMappingEngine>().Use(Mapper.Engine);

                For<IEmailAddress>().Use<EmailAddress>();
                For<IEmailPackager>().Use<SendGridPackager>();
                For<INotification>().Use<Hub.Services.Notification>();

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
                For<InternalInterfaces.IContainer>().Use<InternalClass.Container>();
                For<ICriteria>().Use<Criteria>();
                For<ISubscription>().Use<Subscription>();
                For<IAction>().Use<Action>();
					 For<IRouteNode>().Use<RouteNode>();

                For<IProcessNode>().Use<ProcessNode>();
                For<IRoute>().Use<Route>();
                For<ISubroute>().Use<Subroute>();
                For<IField>().Use<Field>();
                //var mockProcess = new Mock<IProcessService>();
                //mockProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
                //For<IProcessService>().Use(mockProcess.Object);
                //For<Mock<IProcessService>>().Use(mockProcess);

                var terminalTransmitterMock = new Mock<ITerminalTransmitter>();
                For<ITerminalTransmitter>().Use(terminalTransmitterMock.Object).Singleton();
                For<IActivityTemplate>().Use<ActivityTemplate>();
                For<IEvent>().Use<Event>();
                //For<ITemplate>().Use<Services.Template>();
                For<IFile>().Use<File>();
                For<ITerminal>().Use<Terminal>();
                For<ICrateManager>().Use<CrateManager>();
                For<IFr8Event>().Use<Fr8Event>();
                For<IManifest>().Use<Manifest>();
                For<IFindObjectsRoute>().Use<FindObjectsRoute>();
                For<IAuthorization>().Use<Authorization>();

				var timeMock = new Mock<ITime>();
	            For<ITime>().Use(timeMock.Object);

				var pusherNotifierMock = new Mock<IPusherNotifier>();
	            For<IPusherNotifier>().Use(pusherNotifierMock.Object).Singleton();
            }
        }

        #endregion
    }
}