using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Managers;
using HubWeb.Controllers;
using Utilities.Serializers.Json;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using System;
using Fr8Data.DataTransferObjects;
using Hub.Crates.Helpers;

namespace HubTests.Controllers
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

    }
}
