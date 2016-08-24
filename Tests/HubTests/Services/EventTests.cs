using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;
using Data.Repositories.Plan;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Hub.Services;
using Moq;
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

            var fr8AccountMock = new Mock<IFr8Account>();
            fr8AccountMock.Setup(x => x.GetSystemUser())
                    .Returns(() => new Fr8AccountDO() {UserName = "test@test.com", EmailAddress = new EmailAddressDO()});
            ObjectFactory.Container.Inject(typeof(IFr8Account),fr8AccountMock.Object);

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

        public Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO currentActivityDo)
        {
            return _activity.SaveOrUpdateActivity(currentActivityDo);
        }

        public Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO curActivityDO)
        {
            return _activity.Configure(uow, userId, curActivityDO);
        }

        public ActivityDO GetById(IUnitOfWork uow, Guid id)
        {
            return _activity.GetById(uow, id);
        }



        public Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, Guid activityTemplateId, string label = null, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null, PlanVisibility newPlanVisibility = PlanVisibility.Standard)
        {
            return _activity.CreateAndConfigure(uow, userId, activityTemplateId, label, name, order, parentNodeId, createPlan, authorizationTokenId, newPlanVisibility);
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
            return Task.FromResult(Mapper.Map<ActivityDTO>(curActivityDO));
        }

        public Task Deactivate(ActivityDO curActivityDO)
        {
            return Task.FromResult(Mapper.Map<ActivityDTO>(curActivityDO));
        }

        Task<T> IActivity.GetActivityDocumentation<T>(ActivityDTO curActivityDTO, bool isSolution)
        {
            return _activity.GetActivityDocumentation<T>(curActivityDTO, isSolution);
        }

        public List<string> GetSolutionNameList(string terminalName)
        {
            return _activity.GetSolutionNameList(terminalName);
        }

        public Task Delete(Guid id)
        {
            return _activity.Delete(id);
        }

        public Task DeleteChildNodes(Guid id)
        {
            return _activity.DeleteChildNodes(id);
        }

        public bool Exists(Guid id)
        {
            return _activity.Exists(id);
        }

        public Task<ActivityDO> GetSubordinateActivity(IUnitOfWork uow, Guid id)
        {
            return _activity.GetSubordinateActivity(uow, id);
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

        public List<CrateDescriptionCM> GetCrateManifestsByDirection(Guid activityId, CrateDirection direction,
            AvailabilityType availability, bool includeCratesFromActivity = true)
        {
            throw new NotImplementedException();
        }

        public IncomingCratesDTO GetIncomingData(Guid activityId, CrateDirection direction, AvailabilityType availability)
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

        public IEnumerable<ActivityTemplateCategoryDTO> GetActivityTemplatesGroupedByCategories()
        {
            throw new NotImplementedException();
        }
    }
}
