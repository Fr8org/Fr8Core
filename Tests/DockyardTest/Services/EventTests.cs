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

namespace DockyardTest.Services
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

            var activityMock = new PlanNodeMock(plan1, plan2);

            ObjectFactory.Container.Inject(typeof(IPlanNode), activityMock);

            //Act
            var eventService = new Event();
            var curCrateStandardEventReport = ObjectFactory.GetInstance<ICrateManager>().FromDto(FixtureData.CrateDTOForEvents(externalAccountId));
            await eventService.ProcessInboundEvents(curCrateStandardEventReport);

            Assert.AreEqual(2, activityMock.Processed);
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

    public class PlanNodeMock : IPlanNode
    {
        public int Processed;
        private readonly Dictionary<Guid, PlanDO> _planNodes = new Dictionary<Guid, PlanDO>();
        private readonly HashSet<Guid> _plans = new HashSet<Guid>();
        private readonly ICrateManager _crate;

        public PlanNodeMock(params PlanDO[] plans)
        {
            foreach (var planDo in plans)
            {
                _crate = ObjectFactory.GetInstance<ICrateManager>();
                _plans.Add(planDo.Id);
                var plan = planDo;
                PlanTreeHelper.Visit(planDo, x => _planNodes[x.Id] = plan);
            }
        }

        public List<T> GetCrateManifestsByDirection<T>(Guid activityId, CrateDirection direction,
            AvailabilityType availability) where T : Manifest
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

        public Task Process(Guid curActivityId, ActivityState curActionState, ContainerDO curContainerDO)
        {
            if (PlanTreeHelper.Linearize(_planNodes[curActivityId]).OfType<ActivityDO>().Any(x => x.Id == curActivityId))
            {
                Processed++;

                //                    using (var storage = _crate.GetUpdatableStorage(() => curContainerDO.CrateStorage))
                //                    {
                //                        var operationalState = storage.CrateStorage.CrateContentsOfType<OperationalStateCM>().Single();
                //                        operationalState.CurrentActivityResponse = ActivityResponse.Success;
                //                    }
            }

            return Task.Delay(1);
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
            return _planNodes[currentActivity.Id].ChildNodes.First().ChildNodes.First();
        }

        public bool HasChildren(PlanNodeDO currentActivity)
        {
            return _planNodes[currentActivity.Id].StartingSubPlanId == currentActivity.Id;
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
