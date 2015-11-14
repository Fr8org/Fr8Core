using System.Threading.Tasks;
using System.Web.Http.Results;
using Data.Crates;
using Data.Interfaces.Manifests;
using NUnit.Framework;
using terminalPapertrial.Controllers;
using UtilitiesTesting;

namespace terminalPapertrial.Tests.Controllers
{
    [TestFixture]
    [Category("terminalPapertrialControllers")]
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
            var result = _terminal_controller.Get();

            //Assert
            Assert.IsNotNull(result, "The terminal discovery is failed for Terminal Papertrial");
            Assert.IsNotNull((result as JsonResult<StandardFr8TerminalCM>).Content);
        }

        [Test]
        public async Task Get_ShouldReturn_WriteToLogActivityTemplate()
        {
            //Act
            var result = _terminal_controller.Get();

            //Assert
            var actions = (result as JsonResult<StandardFr8TerminalCM>).Content.Actions;
            Assert.AreEqual(1, actions.Count, "Terminal paper trial has more than one actions.");
            Assert.AreEqual("Write_To_Log", actions[0].Name,
                "Write to log action for Papertrial terminal is not avaialble.");
        }
    }
}