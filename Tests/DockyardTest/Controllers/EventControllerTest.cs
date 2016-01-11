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
            _eventReporter = new EventReporter();
            _incidentReporter = new IncidentReporter();
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
            var curRoute = FixtureData.TestRouteWithSubscribeEvent(FixtureData.TestDockyardAccount1());
            FixtureData.TestRouteWithSubscribeEvent(FixtureData.TestDeveloperAccount());
            FixtureData.AddAuthorizationToken(FixtureData.TestDockyardAccount1(), externalAccountId);
            FixtureData.AddAuthorizationToken(FixtureData.TestDeveloperAccount(), externalAccountId);

            //Create activity mock to process the actions
            Mock<IRouteNode> activityMock = new Mock<IRouteNode>(MockBehavior.Default);
            activityMock.Setup(a => a.Process(FixtureData.GetTestGuidById(1), It.IsAny<ActionState>(), It.IsAny<ContainerDO>())).Returns(Task.Delay(1));
            activityMock.Setup(a => a.HasChildren(It.Is<RouteNodeDO>(r => r.Id == curRoute.StartingSubroute.Id))).Returns(true);
            activityMock.Setup(a => a.HasChildren(It.Is<RouteNodeDO>(r => r.Id != curRoute.StartingSubroute.Id))).Returns(false);
            activityMock.Setup(a => a.GetFirstChild(It.IsAny<RouteNodeDO>())).Returns(curRoute.ChildNodes.First().ChildNodes.First());
            ObjectFactory.Container.Inject(typeof(IRouteNode), activityMock.Object);

            //Act
            EventController eventController = new EventController();
            await eventController.ProcessEvents(FixtureData.CrateDTOForEvents(externalAccountId));

            //Assert
            activityMock.Verify(activity => activity.Process(FixtureData.GetTestGuidById(1), It.IsAny<ActionState>(), It.IsAny<ContainerDO>()), Times.Exactly(2));
        }
        
    }
}
