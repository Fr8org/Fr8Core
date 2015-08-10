
using System.Linq;
using System.Web.Http.Results;
using Core.Managers;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;

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
    }
}
