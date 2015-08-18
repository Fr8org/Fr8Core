
using System.Linq;
using System.Web.Http.Results;
using Core.Managers;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Data.Entities;
using System.Collections.Generic;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class EventControllerTest : BaseTest
    {
        private EventController _eventController;
        private EventReporter _eventReporter;
        private IncidentReporter _incidentReporter;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _eventController = new EventController();
            _eventReporter = new EventReporter();
            _incidentReporter = new IncidentReporter();
        }

        [Test]
        [Category("Controllers.EventController.Event")]
        public void EventController_Event_WithPluginIncident_ReturnsOK()
        {
            //Arrange with plugin incident
            _incidentReporter.SubscribeToAlerts();
            var eventDto = FixtureData.TestPluginIncidentDto();

            //Act
          
            var result = _eventController.Event(eventDto);

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
                var result = _eventController.Event(curEventDTO);

                //Assert
                Assert.IsTrue(result is OkResult);

                List<FactDO> savedFactDoList = uow.FactRepository.GetAll().ToList();
                Assert.AreEqual(1, savedFactDoList.Count());
                Assert.AreEqual(curEventDTO.Data.PrimaryCategory, savedFactDoList[0].PrimaryCategory);
                Assert.AreEqual(curEventDTO.Data.SecondaryCategory, savedFactDoList[0].SecondaryCategory);
                _eventReporter.UnsubscribeFromAlerts();
            }

        }
    }
}
