using NUnit.Framework;
using HubWeb.Controllers;
using Fr8.Testing.Unit;
using System.Threading.Tasks;
using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubTests.Controllers
{
    [TestFixture]
    public class EventControllerTest : BaseTest
    {
        private EventsController _eventController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _eventController = new EventsController();
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task Events_NullCrateDTO_ThrowsException()
        {
            await _eventController.Post(null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task Events_NotStandardEventReport_ThrowsException()
        {
            var crateDTO = new CrateDTO();
            await _eventController.Post(crateDTO);
        }
        
    }
}
