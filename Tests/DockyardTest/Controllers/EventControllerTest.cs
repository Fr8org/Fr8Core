
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
        [Test]
        [Category("Controllers.EventController.Event")]
        public void EventController_Event_WithPluginIncident_ReturnsOK()
        {
            //Arrange with plugin incident
            new IncidentReporter().SubscribeToAlerts();
            var eventDto = FixtureData.TestPluginIncidentDto();

            //Act
            var controller = new EventController();
            var result = controller.Event(eventDto);

            //Assert
            Assert.IsTrue(result is OkResult);

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            Assert.AreEqual(1, uow.IncidentRepository.GetAll().Count());
        }

        //[Test]
        //[Category("Controllers.EventController.Event")]
        //public void EventController_Event_WithPluginEvent_ReturnsOK()
        //{
        //    //Arrange with plugin event
        //    new EventReporter().SubscribeToAlerts();
        //    var eventDto = FixtureData.TestPluginEventDto();

        //    //Act
        //    var controller = new EventController();
        //    var result = controller.Event(eventDto);

        //    //Assert
        //    Assert.IsTrue(result is OkResult);

        //    var uow = ObjectFactory.GetInstance<IUnitOfWork>();
        //    List<FactDO> savedFactDoList=uow.FactRepository.GetAll().ToList();
        //    Assert.AreEqual(1, savedFactDoList.Count());
        //    Assert.AreEqual(eventDto.Data.PrimaryCategory, savedFactDoList[0].PrimaryCategory);
        //    Assert.AreEqual(eventDto.Data.SecondaryCategory, savedFactDoList[0].SecondaryCategory);
        //    uow.FactRepository.Remove(savedFactDoList[0]);
        //    uow.SaveChanges();

        //}
    }
}
