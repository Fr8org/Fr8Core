using NUnit.Framework;
using HubWeb.Controllers;
using Fr8.Testing.Unit;
using System.Threading.Tasks;
using System;
using Fr8.Infrastructure.Data.DataTransferObjects;
using System.Web.Http.Results;

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
        public async Task Events_NullCrateDTO_ThrowsException()
        {
            var result = await _eventController.Post(null);
            Assert.IsTrue(result is BadRequestErrorMessageResult, "Post method was expected to return BadRequest (code 400) for null payload");
        }

        [Test]
        public async Task Events_NotStandardEventReport_ThrowsException()
        {
            var crateDTO = new CrateDTO();
            var result = await _eventController.Post(crateDTO);
            Assert.IsTrue(result is BadRequestErrorMessageResult, "Post method was expected to return BadRequest (code 400) for empty payload");
        }
        
    }
}
