using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Crates.Helpers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Managers;
using HubWeb.Controllers;
using Utilities.Serializers.Json;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using System;
using Data.Constants;
using Data.Interfaces.Manifests;
using Data.Repositories.Plan;
using Data.States;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class EventControllerTest : BaseTest
    {
        private EventController _eventController;
        private EventReporter _eventReporter;
        private IncidentReporter _incidentReporter;
        private EventReportCrateFactory _eventReportCrateFactoryHelper;
        private ICrateManager _crate;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _eventController = new EventController();
            _eventReporter = ObjectFactory.GetInstance <EventReporter>();
            _incidentReporter = ObjectFactory.GetInstance <IncidentReporter>();
            _eventReportCrateFactoryHelper = new EventReportCrateFactory();
            _crate = ObjectFactory.GetInstance<ICrateManager>();

        }

        [Test]
        [Category("Controllers.EventController.Event")]
        public void EventController_Event_WithTerminalIncident_ReturnsOK()
        {
            //Arrange with terminal incident
            _incidentReporter.SubscribeToAlerts();
            var eventDto = FixtureData.TestTerminalIncidentDto();

            //Act

            var result = _eventController.ProcessGen1Event(_crate.ToDto(_eventReportCrateFactoryHelper.Create(eventDto)));

            //Assert
            Assert.IsTrue(result is OkResult);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var redfsasult = uow.IncidentRepository.GetAll();

            Assert.AreEqual(1, uow.IncidentRepository.GetAll().Count());
        }

        [Test]
        [Category("Controllers.EventController.Event")]
        public void EventController_Post_WithTerminalEvent_ReturnsOK()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange with terminal event
                _eventReporter.SubscribeToAlerts();

                var curEventDTO = FixtureData.TestTerminalEventDto();

                //Act
                var result = _eventController.ProcessGen1Event(_crate.ToDto(_eventReportCrateFactoryHelper.Create(curEventDTO)));

                //Assert
                Assert.IsTrue(result is OkResult);
                List<FactDO> savedFactDoList = uow.FactRepository.GetAll().ToList();
                Assert.AreEqual(1, savedFactDoList.Count());
                var loggingData = curEventDTO.CrateStorage.First().Get<LoggingDataCm>();
                Assert.AreEqual(loggingData.PrimaryCategory, savedFactDoList[0].PrimaryCategory);
                Assert.AreEqual(loggingData.SecondaryCategory, savedFactDoList[0].SecondaryCategory);
                _eventReporter.UnsubscribeFromAlerts();
            }

        }

        //[Test]
        //[Category("Controllers.EventController.Event")]
        //public void EventController_ProcessIncomingEvents_WithPluginEvent_RetunsOK()
        //{
        //    //setup mock Event
        //    var mockEvent = new Mock<IEvent>();

        //    mockEvent.Setup(e => e.HandlePluginEvent(It.IsAny<LoggingData>()));

        //    _eventController.ProcessIncomingEvents(mockEvent);
        //}
        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task Events_NullCrateDTO_ThrowsException()
        {
            await _eventController.ProcessEvents(null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task Events_NotStandardEventReport_ThrowsException()
        {
            var crateDTO = new CrateDTO();
            await _eventController.ProcessEvents(crateDTO);
        }

        //[Test]
        //public void fr8_events_CorrectStandardEventReport_ReturnsOK()
        //{
        //    Mock<IDockyardEvent> dockyardEventMock = new Mock<IDockyardEvent>();
        //    dockyardEventMock.Setup(a => a.ProcessInbound("1", It.IsAny<EventReportMS>()));
        //    ObjectFactory.Configure(cfg => cfg.For<IDockyardEvent>().Use(dockyardEventMock.Object));
        //    var dockyardEventController = new DockyardEventController();

        //    var actionResult = dockyardEventController.ProcessDockyardEvents(FixtureData.RawStandardEventReportFormat());

        //    Assert.IsNotNull(actionResult);
        //}

        [Test]
        public async Task Events_Multiplefr8AccountsAssociatedWithSameExternalAccountId_ShouldCheckRoutesForAllUsers()
        {
            //Arrange 
            var externalAccountId = "docusign_developer@dockyard.company";
            var plan1 = FixtureData.TestRouteWithSubscribeEvent(FixtureData.TestDockyardAccount1());
            var plan2 = FixtureData.TestRouteWithSubscribeEvent(FixtureData.TestDeveloperAccount(), 23);
            FixtureData.AddAuthorizationToken(FixtureData.TestDockyardAccount1(), externalAccountId);
            FixtureData.AddAuthorizationToken(FixtureData.TestDeveloperAccount(), externalAccountId);

            var activityMock = new RouteNodeMock(plan1, plan2);

            ObjectFactory.Container.Inject(typeof(IRouteNode), activityMock);

            //Act
            EventController eventController = new EventController();
            await eventController.ProcessEvents(FixtureData.CrateDTOForEvents(externalAccountId));

            Assert.AreEqual(2, activityMock.Processed);
        }

        public class RouteNodeMock : IRouteNode
        {
            public int Processed;
            private readonly Dictionary<Guid, PlanDO> _planNodes = new Dictionary<Guid, PlanDO>(); 
            private readonly HashSet<Guid> _plans = new HashSet<Guid>(); 

            public RouteNodeMock(params PlanDO[] plans)
            {
                foreach (var planDo in plans)
                {
                    _plans.Add(planDo.Id);
                    var plan = planDo;
                    RouteTreeHelper.Visit(planDo, x=>_planNodes[x.Id] = plan);
                }
            }

            public List<RouteNodeDO> GetUpstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO)
            {
                throw new NotImplementedException();
            }

            public List<RouteNodeDO> GetDownstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO)
            {
                throw new NotImplementedException();
            }

            public StandardDesignTimeFieldsCM GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability)
            {
                throw new NotImplementedException();
            }

            public Task Process(Guid curActivityId, ActionState curActionState, ContainerDO curContainerDO)
            {
                if (RouteTreeHelper.Linearize(_planNodes[curActivityId]).OfType<ActivityDO>().Any(x=>x.Id == curActivityId))
                {
                    Processed++;
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

            public RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, RouteNodeDO root)
            {
                return null;
            }

            public RouteNodeDO GetNextSibling(RouteNodeDO currentActivity)
            {
                return null;
            }

            public RouteNodeDO GetParent(RouteNodeDO currentActivity)
            {
                return null;
            }

            public RouteNodeDO GetFirstChild(RouteNodeDO currentActivity)
            {
                return _planNodes[currentActivity.Id].ChildNodes.First().ChildNodes.First();
            }

            public bool HasChildren(RouteNodeDO currentActivity)
            {
                return _planNodes[currentActivity.Id].StartingSubrouteId == currentActivity.Id;
            }

            public void Delete(IUnitOfWork uow, RouteNodeDO activity)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivitiyGroups()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow, IFr8AccountDO curAccount)
            {
                throw new NotImplementedException();
            }
        }

    }
}
