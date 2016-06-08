using System.Threading.Tasks;
using System.Web.Http.Results;
using fr8.Infrastructure.Data.Manifests;
using NUnit.Framework;
using terminalPapertrail.Controllers;
using UtilitiesTesting;

namespace terminalPapertrail.Tests.Controllers
{
    [TestFixture]
    [Category("terminalPapertrailControllers")]
    public class TerminalControllerTests : BaseTest
    {
        private TerminalController _terminal_controller;

        public override void SetUp()
        {
            base.SetUp();
            _terminal_controller = new TerminalController();
        }

        [Test]
        public async Task Get_ShouldReturn_NonEmptyJsonResult()
        {
            //Act
            var result = _terminal_controller.DiscoverTerminals();

            //Assert
            Assert.IsNotNull(result, "The terminal discovery is failed for Terminal Papertrail");
            Assert.IsNotNull((result as JsonResult<StandardFr8TerminalCM>).Content);
        }

        [Test]
        public async Task Get_ShouldReturn_WriteToLogActivityTemplate()
        {
            //Act
            var result = _terminal_controller.DiscoverTerminals();

            //Assert
            var actions = (result as JsonResult<StandardFr8TerminalCM>).Content.Activities;
            Assert.AreEqual(1, actions.Count, "Terminal paper trial has more than one actions.");
            Assert.AreEqual("Write_To_Log", actions[0].Name,
                "Write to log activity for Papertrail terminal is not avaialble.");
        }
    }
}