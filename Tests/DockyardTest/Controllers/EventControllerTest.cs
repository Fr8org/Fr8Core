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
        public void EventController_Event_WithPluginIncident_ReturnsOK()
        {
            //Arrange with plugin incident
            _incidentReporter.SubscribeToAlerts();
            var eventDto = FixtureData.TestPluginIncidentDto();

            //Act

            var result = _eventController.Post(_eventReportCrateFactoryHelper.Create(eventDto));

            //Assert
            Assert.IsTrue(result is OkResult);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var redfsasult = uow.IncidentRepository.GetAll();

            Assert.AreEqual(1, uow.IncidentRepository.GetAll().Count());
        }

        [Test]
        [Category("Controllers.EventController.Event")]
        public void EventController_Post_WithPluginEvent_ReturnsOK()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange with plugin event
                _eventReporter.SubscribeToAlerts();

                var curEventDTO = FixtureData.TestPluginEventDto();

                //Act
                var result = _eventController.Post(_eventReportCrateFactoryHelper.Create(curEventDTO));

                //Assert
                Assert.IsTrue(result is OkResult);
                List<FactDO> savedFactDoList = uow.FactRepository.GetAll().ToList();
                Assert.AreEqual(1, savedFactDoList.Count());
                var loggingData = _crate.GetContents<LoggingData>(curEventDTO.CrateStorage.First());
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


        
    }
}
