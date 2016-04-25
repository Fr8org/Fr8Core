using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.Manifests;
using Data.Repositories.Plan;
using Data.States;
using Hub.Managers;
using Event = Hub.Services.Event;

namespace HubTests.Services
{
    [TestFixture]
    public class DockyardEventTests : BaseTest
    {
        [Test]
        public async Task Events_Multiplefr8AccountsAssociatedWithSameExternalAccountId_ShouldCheckPlansForAllUsers()
        {
            //Arrange 
            var externalAccountId = "docusign_developer@dockyard.company";
            var plan1 = FixtureData.TestPlanWithSubscribeEvent(FixtureData.TestDockyardAccount1());
            var plan2 = FixtureData.TestPlanWithSubscribeEvent(FixtureData.TestDeveloperAccount(), 23);
            FixtureData.AddAuthorizationToken(FixtureData.TestDockyardAccount1(), externalAccountId);
            FixtureData.AddAuthorizationToken(FixtureData.TestDeveloperAccount(), externalAccountId);

            var planNode = new PlanNodeMock(plan1, plan2);
            var activity = new ActivityMock(ObjectFactory.GetInstance<IActivity>(), planNode.PlanNodes);

            ObjectFactory.Container.Inject(typeof(IPlanNode), planNode);
            ObjectFactory.Container.Inject(typeof(IActivity), activity);

            //Act
            var eventService = new Event();
            var curCrateStandardEventReport = ObjectFactory.GetInstance<ICrateManager>().FromDto(FixtureData.CrateDTOForEvents(externalAccountId));
            await eventService.ProcessInboundEvents(curCrateStandardEventReport);
            Assert.AreEqual(2, activity.Processed);
        }
        //[Test]
        //[ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        //public void ProcessInbound_EmptyUserID()
        //{
        //    IDockyardEvent curDockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();

        //    curDockyardEvent.ProcessInbound("", new EventReportMS());
        //}

        //[Test]
        //public void ProcessInbound_CorrectStandardEventReportLabel_CallLaunchProcess()
        //{
        //    var processTemplateDO = FixtureData.TestPlanWithSubscribeEvent();
        //    var resultRoutes = new List<RouteDO>() { processTemplateDO };
        //    IRoute curPlan = ObjectFactory.GetInstance<IRoute>();
        //    EventReportMS curEventReport = FixtureData.StandardEventReportFormat();

        //    Mock<IRoute> processTemplateMock = new Mock<IRoute>();
        //    processTemplateMock.Setup(a => a.LaunchProcess(It.IsAny<IUnitOfWork>(), It.IsAny<RouteDO>(), null));
        //    processTemplateMock.Setup(a => a.GetMatchingRoutes(It.IsAny<string>(), It.IsAny<EventReportMS>()))
        //        .Returns(resultRoutes);
        //    ObjectFactory.Configure(cfg => cfg.For<IRoute>().Use(processTemplateMock.Object));

        //    IDockyardEvent curDockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();

        //    curDockyardEvent.ProcessInbound("testuser1", curEventReport);

        //    processTemplateMock.Verify(l => l.LaunchProcess(It.IsAny<IUnitOfWork>(), It.IsAny<RouteDO>(), null));
        //}
    }

    public class ActivityMock : IActivity
    {
        private readonly IActivity _activity;
        private readonly Dictionary<Guid, PlanDO> _planNodes;

        public int Processed { get; set; }

        public ActivityMock(IActivity activity, Dictionary<Guid, PlanDO> planNodes)
        {
            _activity = activity;
            _planNodes = planNodes;
        }

        public IEnumerable<TViewModel> GetAllActivities<TViewModel>()
        {
            return _activity.GetAllActivities<TViewModel>();
        }

        public Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO currentActivityDo)
        {
            return _activity.SaveOrUpdateActivity(currentActivityDo);
        }

        public Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO curActivityDO, bool saveResult = true)
        {
            return _activity.Configure(uow, userId, curActivityDO, saveResult);
        }

        public ActivityDO GetById(IUnitOfWork uow, Guid id)
        {
            return _activity.GetById(uow, id);
        }

        public ActivityDO MapFromDTO(ActivityDTO curActivityDTO)
        {
            return _activity.MapFromDTO(curActivityDTO);
        }

        public Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, Guid actionTemplateId, string label = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
        {
            return _activity.CreateAndConfigure(uow, userId, actionTemplateId, label, order, parentNodeId, createPlan, authorizationTokenId);
        }

        public async Task<PayloadDTO> Run(IUnitOfWork uow, ActivityDO curActivityDO, ActivityExecutionMode curActionExecutionMode, ContainerDO curContainerDO)
        {
            if (PlanTreeHelper.Linearize(_planNodes[curActivityDO.Id]).OfType<ActivityDO>().Any(x => x.Id == curActivityDO.Id))
            {
                Processed++;
            }

            await Task.Delay(1);

            return await _activity.Run(uow, curActivityDO, curActionExecutionMode, curContainerDO);
        }

        public Task<ActivityDTO> Activate(ActivityDO curActivityDO)
        {
            return _activity.Activate(curActivityDO);
        }

        public Task<ActivityDTO> Deactivate(ActivityDO curActivityDO)
        {
            return _activity.Deactivate(curActivityDO);
        }

        Task<T> IActivity.GetActivityDocumentation<T>(ActivityDTO curActivityDTO, bool isSolution)
        {
            return _activity.GetActivityDocumentation<T>(curActivityDTO, isSolution);
        }

        public List<string> GetSolutionNameList(string terminalName)
        {
            return _activity.GetSolutionNameList(terminalName);
        }

        public void Delete(Guid id)
        {
            _activity.Delete(id);
        }
    }

    public class PlanNodeMock : IPlanNode
    {
        public readonly Dictionary<Guid, PlanDO> PlanNodes = new Dictionary<Guid, PlanDO>();
        private readonly HashSet<Guid> _plans = new HashSet<Guid>();

        public PlanNodeMock(params PlanDO[] plans)
        {
            foreach (var planDo in plans)
            {
                _plans.Add(planDo.Id);
                var plan = planDo;
                PlanTreeHelper.Visit(planDo, x => PlanNodes[x.Id] = plan);
            }
        }

        public List<T> GetCrateManifestsByDirection<T>(Guid activityId, CrateDirection direction,
            AvailabilityType availability, bool includeCratesFromActivity = true) where T : Manifest
        {
            throw new NotImplementedException();
        }

        public IncomingCratesDTO GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            throw new NotImplementedException();
        }

        public List<PlanNodeDO> GetUpstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO)
        {
            throw new NotImplementedException();
        }

        public List<PlanNodeDO> GetDownstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO)
        {
            throw new NotImplementedException();
        }

        public FieldDescriptionsCM GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, Func<ActivityTemplateDO, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public PlanNodeDO GetNextActivity(PlanNodeDO currentActivity, PlanNodeDO root)
        {
            return null;
        }

        public PlanNodeDO GetNextSibling(PlanNodeDO currentActivity)
        {
            return null;
        }

        public PlanNodeDO GetParent(PlanNodeDO currentActivity)
        {
            return null;
        }

        public PlanNodeDO GetFirstChild(PlanNodeDO currentActivity)
        {
            return PlanNodes[currentActivity.Id].ChildNodes.First().ChildNodes.First();
        }

        public bool HasChildren(PlanNodeDO currentActivity)
        {
            return PlanNodes[currentActivity.Id].StartingSubPlanId == currentActivity.Id;
        }

        public void Delete(IUnitOfWork uow, PlanNodeDO activity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivityGroups()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow, IFr8AccountDO curAccount)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow)
        {
            throw new NotImplementedException();
        }
    }
}
