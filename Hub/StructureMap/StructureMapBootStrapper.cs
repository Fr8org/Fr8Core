using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using AutoMapper;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using Hub.Managers.APIManagers.Packagers.SendGrid;
using InternalInterfaces = Hub.Interfaces;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Packagers;
using Hub.Managers.APIManagers.Packagers.SegmentIO;
using Hub.Managers.APIManagers.Transmitters.Terminal;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using InternalClass = Hub.Services;
using Hub.Services;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Hub.ExternalServices;
using Hub.Security;
using Moq;
// This is used to avoid ambiguity between StructureMap.IContainer and  Core.Interfaces.IContainer
using ExtternalStructureMap = StructureMap;
using StructureMap;
using StructureMap.Configuration.DSL;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.ApplicationInsights;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using Data.Interfaces;
using Data.Repositories.Utilization;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities;
using Hub.Security.ObjectDecorators;
using Hub.Services.PlanDirectory;
using Hub.Services.Timers;

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
                    ObjectFactory.Configure(x => x.AddRegistry<LiveMode>());
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
        
        public class LiveMode : DatabaseStructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<ITerminalDiscoveryService>().Use<TerminalDiscoveryService>().Singleton();
                For<IConfigRepository>().Use<ConfigRepository>();
                For<IMappingEngine>().Use(Mapper.Engine);

                For<IEmailAddress>().Use<EmailAddress>();
                For<IEmailPackager>().Use<SendGridPackager>();
                For<INotification>().Use<Hub.Services.Notification>();

                For<ISecurityServices>().Use<SecurityServices>();
                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();


                For<IImapClient>().Use<ImapClientWrapper>();
                For<ITerminalTransmitter>().Use<TerminalTransmitter>();

                For<IPlan>().Use<Hub.Services.Plan>().DecorateWith((context, service) => new PlanSecurityDecorator(service, ObjectFactory.GetInstance<ISecurityServices>()));
                For<InternalInterfaces.IContainerService>().Use<InternalClass.ContainerService>();
                For<InternalInterfaces.IFact>().Use<InternalClass.Fact>();
                var dynamicProxy = new ProxyGenerator();
                For<IActivity>().Use<Activity>().Singleton().DecorateWith(z => dynamicProxy.CreateInterfaceProxyWithTarget(z, new AuthorizeActivityInterceptor(ObjectFactory.GetInstance<ISecurityServices>())));
                For<IPlanNode>().Use<PlanNode>();
                For<ISubscription>().Use<Subscription>();
                For<ISubplan>().Use<Subplan>();
                //For<IDocuSignTemplate>().Use<DocuSignTemplate>();
                For<IEvent>().Use<Hub.Services.Event>();
                For<IActivityTemplate>().Use<ActivityTemplate>().Singleton();
                For<IActivityCategory>().Use<ActivityCategory>().Singleton();
                For<IFile>().Use<InternalClass.File>();
                For<ITerminal>().Use<Terminal>().Singleton();
                For<ICrateManager>().Use<CrateManager>();
                For<IReport>().Use<Report>();
                For<IManifest>().Use<ManifestService>();
                For<ITime>().Use<Time>();
                For<IPusherNotifier>().Use<PusherNotifier>();
                For<IAuthorization>().Use<Authorization>();
                For<ITag>().Use<Tag>();
                For<IOrganization>().Use<Organization>();
                For<IPageDefinition>().Use<PageDefinition>();

                For<TelemetryClient>().Use<TelemetryClient>();
                For<IJobDispatcher>().Use<HangfireJobDispatcher>();
                // For<Hub.Managers.Event>().Use<Hub.Managers.Event>().Singleton();
                For<IUtilizationMonitoringService>().Use<UtilizationMonitoringService>().Singleton();
                For<IActivityExecutionRateLimitingService>().Use<ActivityExecutionRateLimitingService>().Singleton();
                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();
                For<ITimer>().Use<Win32Timer>();
                For<IManifestRegistryMonitor>().Use<ManifestRegistryMonitor>().Singleton();
                For<IUpstreamDataExtractionService>().Use<UpstreamDataExtractionService>().Singleton();


                //PD services
                For<ITagGenerator>().Use<TagGenerator>().Singleton();
                For<IPlanTemplate>().Use<PlanTemplate>().Singleton();
                For<ISearchProvider>().Use<SearchProvider>().Singleton();
                For<IPageDefinition>().Use<PageDefinition>().Singleton();
                //For<IPageDefinitionRepository>().Use<PageDefinitionRepository>().Singleton();

                For<IPlanDirectoryService>().Use<PlanDirectoryService>().Singleton();
                
            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
                For<ITerminalDiscoveryService>().Use<TerminalDiscoveryService>().Singleton();
                For<IConfigRepository>().Use<MockedConfigRepository>();
                For<IMappingEngine>().Use(Mapper.Engine);

                For<IEmailAddress>().Use<EmailAddress>();
                For<IEmailPackager>().Use<SendGridPackager>();
                For<INotification>().Use<Hub.Services.Notification>();

                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<ISecurityServices>().Use(new MockedSecurityServices());


                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();

                var mockSegment = new Mock<ITracker>();
                For<ITracker>().Use(mockSegment.Object);
                For<InternalInterfaces.IContainerService>().Use<InternalClass.ContainerService>();
                For<InternalInterfaces.IFact>().Use<InternalClass.Fact>();

                For<ISubscription>().Use<Subscription>();
                For<IActivity>().Use<Activity>().Singleton();
                For<IPlanNode>().Use<PlanNode>();

                For<IPlan>().Use<Hub.Services.Plan>();

                For<ISubplan>().Use<Subplan>();
                For<IFr8Account>().Use<Fr8Account>();
                //var mockProcess = new Mock<IProcessService>();
                //mockProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
                //For<IProcessService>().Use(mockProcess.Object);
                //For<Mock<IProcessService>>().Use(mockProcess);

                var terminalTransmitterMock = new Mock<ITerminalTransmitter>();
                For<ITerminalTransmitter>().Use(terminalTransmitterMock.Object).Singleton();
                For<IActivityTemplate>().Use<ActivityTemplate>().Singleton();
                For<IActivityCategory>().Use<ActivityCategory>().Singleton();
                For<IEvent>().Use<Hub.Services.Event>();
                //For<ITemplate>().Use<Services.Template>();
                For<IFile>().Use<InternalClass.File>();

                For<ICrateManager>().Use<CrateManager>();

                For<IManifest>().Use<ManifestService>();
                For<IAuthorization>().Use<Authorization>();
                For<IReport>().Use<Report>();
                var timeMock = new Mock<ITime>();
                For<ITime>().Use(timeMock.Object);

                var pusherNotifierMock = new Mock<IPusherNotifier>();
                For<IPusherNotifier>().Use(pusherNotifierMock.Object).Singleton();

                For<ITag>().Use<Tag>();
                For<IOrganization>().Use<Organization>();
                For<IPageDefinition>().Use<PageDefinition>();

                For<TelemetryClient>().Use<TelemetryClient>();
                For<ITerminal>().Use(x=>new TerminalServiceForTests(x.GetInstance<IConfigRepository>(), x.GetInstance<ISecurityServices>())).Singleton();
                For<IJobDispatcher>().Use<MockJobDispatcher>();
                // For<Hub.Managers.Event>().Use<Hub.Managers.Event>().Singleton();
                For<IUtilizationMonitoringService>().Use<UtilizationMonitoringService>().Singleton();
                For<IActivityExecutionRateLimitingService>().Use<ActivityExecutionRateLimitingService>().Singleton();
                For<ITimer>().Use<Win32Timer>();
                For<IUpstreamDataExtractionService>().Use<UpstreamDataExtractionService>().Singleton();

                //PD bootstrap
                //tony.yakovets: will it work? or some tests check generated templates?
                //var templateGenerator = new Mock<ITemplateGenerator>().Object;
                //For<IWebservicesPageGenerator>().Use<WebservicesPageGenerator>().Singleton().Ctor<ITemplateGenerator>().Is(templateGenerator);
                //For<IManifestPageGenerator>().Use<ManifestPageGenerator>().Singleton().Ctor<ITemplateGenerator>().Is(templateGenerator);

                var webservicesPageGeneratorMock = new Mock<IWebservicesPageGenerator>().Object;
                var manifestPageGeneratorMock = new Mock<IManifestPageGenerator>().Object;
                var planTemplateDetailsMock = new Mock<IPlanTemplateDetailsGenerator>().Object;

                For<ITagGenerator>().Use<TagGenerator>().Singleton();
                For<IPlanTemplate>().Use<PlanTemplate>().Singleton();
                For<ISearchProvider>().Use<SearchProvider>().Singleton();
                For<IPageDefinition>().Use<PageDefinition>().Singleton();
                For<ITemplateGenerator>().Use<TemplateGenerator>().Singleton();

                For<IPlanDirectoryService>().Use<PlanDirectoryService>().Singleton()
                    .Ctor<IWebservicesPageGenerator>().Is(webservicesPageGeneratorMock)
                    .Ctor<IManifestPageGenerator>().Is(manifestPageGeneratorMock)
                    .Ctor<IPlanTemplateDetailsGenerator>().Is(planTemplateDetailsMock);
            }
        }

        public class MockJobDispatcher : IJobDispatcher
        {
            public void Enqueue(Expression<Action> job)
            {
                job.Compile().Invoke();
            }
        }

        public class TerminalServiceForTests : ITerminal
        {
            private readonly ITerminal _terminal;

            public TerminalServiceForTests(IConfigRepository configRepository, ISecurityServices securityServices)
            {
                _terminal = new Terminal(configRepository, securityServices);
            }

            public Dictionary<string, string> GetRequestHeaders(TerminalDO terminal, string userId)
            {
                return new Dictionary<string, string>();
            }

            public Task<TerminalDO> GetByToken(string token)
            {
                return _terminal.GetByToken(token);
            }

            public IEnumerable<TerminalDO> GetAll()
            {
                return _terminal.GetAll();
            }

            public IEnumerable<TerminalDO> GetByCurrentUser()
            {
                return _terminal.GetByCurrentUser();
            }
                
            public Task<IList<ActivityTemplateDO>> GetAvailableActivities(string uri)
            {
                return _terminal.GetAvailableActivities(uri);
            }

            public TerminalDO GetByNameAndVersion(string name, string version)
            {
                return _terminal.GetByNameAndVersion(name, version);
            }

            public TerminalDO RegisterOrUpdate(TerminalDO terminalDo, bool isUserInitiated)
            {
                return _terminal.RegisterOrUpdate(terminalDo, isUserInitiated);
            }

            public TerminalDO GetByKey(Guid terminalId)
            {
                return _terminal.GetByKey(terminalId);
            }
            public Task<List<DocumentationResponseDTO>> GetSolutionDocumentations(string terminalName)
            {
                return _terminal.GetSolutionDocumentations(terminalName);
            }
        }

        #endregion
    }



}