using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using AutoMapper;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using Hub.Managers.APIManagers.Packagers.SendGrid;
using InternalInterfaces = Hub.Interfaces;
using Hub.Interfaces;
using Hub.Managers;
using Hub.Managers.APIManagers.Authorizers;
using Hub.Managers.APIManagers.Authorizers.Google;
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
using Utilities;
using Utilities.Interfaces;
using System.Net.Http;
using Microsoft.ApplicationInsights;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using Data.Interfaces;
using Data.Repositories.Utilization;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Hub.Security.ObjectDecorators;

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
                    ObjectFactory.Configure(x => x.AddRegistry<TestMode>());
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
                For<IConfigRepository>().Use<ConfigRepository>();
                For<IMappingEngine>().Use(Mapper.Engine);

                For<IEmailAddress>().Use<EmailAddress>();
                For<IEmailPackager>().Use<SendGridPackager>();
                For<INotification>().Use<Hub.Services.Notification>();

                For<ISecurityServices>().Use<SecurityServices>();
                For<ITracker>().Use<SegmentIO>();
                For<IIntakeManager>().Use<IntakeManager>();

                For<IOAuthAuthorizer>().Use<GoogleAuthorizer>().Named("Google");

                For<IImapClient>().Use<ImapClientWrapper>();
                For<ITerminalTransmitter>().Use<TerminalTransmitter>();

                For<IPlan>().Use<Hub.Services.Plan>().DecorateWith((context, service) => new PlanSecurityDecorator(service, ObjectFactory.GetInstance<ISecurityServices>()));
                For<InternalInterfaces.IContainer>().Use<InternalClass.Container>();
                For<InternalInterfaces.IFact>().Use<InternalClass.Fact>();
                var dynamicProxy = new ProxyGenerator();
                For<IActivity>().Use<Activity>().Singleton().DecorateWith(z => dynamicProxy.CreateInterfaceProxyWithTarget(z, new AuthorizeActivityInterceptor(ObjectFactory.GetInstance<ISecurityServices>())));
                For<IPlanNode>().Use<PlanNode>();
                For<ISubscription>().Use<Subscription>();
                For<ISubPlan>().Use<SubPlan>();
                For<IField>().Use<Field>();
                //For<IDocuSignTemplate>().Use<DocuSignTemplate>();
                For<IEvent>().Use<Hub.Services.Event>();
                For<IActivityTemplate>().Use<ActivityTemplate>().Singleton();
                For<IFile>().Use<InternalClass.File>();
                For<ITerminal>().Use<Terminal>().Singleton();
                For<ICrateManager>().Use<CrateManager>();
                For<IReport>().Use<Report>();
                For<IManifest>().Use<Manifest>();
                For<IFindObjectsPlan>().Use<FindObjectsPlan>();
                For<ITime>().Use<Time>();
                For<IPusherNotifier>().Use<PusherNotifier>();
                For<IAuthorization>().Use<Authorization>();
                For<ITag>().Use<Tag>();
                For<IOrganization>().Use<Organization>();

                For<TelemetryClient>().Use<TelemetryClient>();
                For<IJobDispatcher>().Use<HangfireJobDispatcher>();
                // For<Hub.Managers.Event>().Use<Hub.Managers.Event>().Singleton();
                For<IPlanTemplates>().Use<PlanTemplates>();
                For<IUtilizationMonitoringService>().Use<UtilizationMonitoringService>().Singleton();
                For<IActivityExecutionRateLimitingService>().Use<ActivityExecutionRateLimitingService>().Singleton();
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

                For<MediaTypeFormatter>().Use<JsonMediaTypeFormatter>();

                var mockSegment = new Mock<ITracker>();
                For<ITracker>().Use(mockSegment.Object);
                For<InternalInterfaces.IContainer>().Use<InternalClass.Container>();
                For<InternalInterfaces.IFact>().Use<InternalClass.Fact>();

                For<ISubscription>().Use<Subscription>();
                For<IActivity>().Use<Activity>().Singleton();
                For<IPlanNode>().Use<PlanNode>();

                For<IPlan>().Use<Hub.Services.Plan>();

                For<ISubPlan>().Use<SubPlan>();
                For<IField>().Use<Field>();
                //var mockProcess = new Mock<IProcessService>();
                //mockProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
                //For<IProcessService>().Use(mockProcess.Object);
                //For<Mock<IProcessService>>().Use(mockProcess);

                var terminalTransmitterMock = new Mock<ITerminalTransmitter>();
                For<ITerminalTransmitter>().Use(terminalTransmitterMock.Object).Singleton();
                For<IActivityTemplate>().Use<ActivityTemplate>().Singleton();
                For<IEvent>().Use<Hub.Services.Event>();
                //For<ITemplate>().Use<Services.Template>();
                For<IFile>().Use<InternalClass.File>();

                For<ICrateManager>().Use<CrateManager>();

                For<IManifest>().Use<Manifest>();
                For<IFindObjectsPlan>().Use<FindObjectsPlan>();
                For<IAuthorization>().Use<Authorization>();
                For<IReport>().Use<Report>();
                var timeMock = new Mock<ITime>();
                For<ITime>().Use(timeMock.Object);

                var pusherNotifierMock = new Mock<IPusherNotifier>();
                For<IPusherNotifier>().Use(pusherNotifierMock.Object).Singleton();

                For<ITag>().Use<Tag>();
                For<IOrganization>().Use<Organization>();
                For<TelemetryClient>().Use<TelemetryClient>();
                For<ITerminal>().Use(new TerminalServiceForTests()).Singleton();
                For<IJobDispatcher>().Use<MockJobDispatcher>();
                // For<Hub.Managers.Event>().Use<Hub.Managers.Event>().Singleton();
                For<IPlanTemplates>().Use<PlanTemplates>();
                For<IUtilizationMonitoringService>().Use<UtilizationMonitoringService>().Singleton();
                For<IActivityExecutionRateLimitingService>().Use<ActivityExecutionRateLimitingService>().Singleton();
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

            public TerminalServiceForTests()
            {
                _terminal = new Terminal();
            }

            public Task<TerminalDO> GetTerminalByPublicIdentifier(string terminalId)
            {
                return Task.FromResult(new TerminalDO());
            }

            public IEnumerable<TerminalDO> GetAll()
            {
                return _terminal.GetAll();
            }

            public Task<IList<ActivityTemplateDO>> GetAvailableActivities(string uri)
            {
                return _terminal.GetAvailableActivities(uri);
            }

            public TerminalDO GetByNameAndVersion(string name, string version)
            {
                return _terminal.GetByNameAndVersion(name, version);
            }

            public TerminalDO RegisterOrUpdate(TerminalDO terminalDo)
            {
                return _terminal.RegisterOrUpdate(terminalDo);
            }

            public TerminalDO GetByKey(int terminalId)
            {
                return _terminal.GetByKey(terminalId);
            }

            public Task<bool> IsUserSubscribedToTerminal(string terminalId, string userId)
            {
                return _terminal.IsUserSubscribedToTerminal(terminalId, userId);

            }

            public Task<List<SolutionPageDTO>> GetSolutionDocumentations(string terminalName)
            {
                return _terminal.GetSolutionDocumentations(terminalName);
            }
        }

        #endregion
    }



}