using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Moq;
using NUnit.Framework;
using HubTests.Controllers.Api;
using Hub.Interfaces;
using HubWeb.Controllers;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("DocumentationController")]
    public class DocumentationControllerTest : ApiControllerTestBase
    {
        private Mock<ITerminal> _terminalMock;

        private Mock<IActivity> _activityMock;

        public override void SetUp()
        {
            base.SetUp();
            _terminalMock = new Mock<ITerminal>();
            _activityMock = new Mock<IActivity>();
        }

        [Test]
        public async Task ActivityDocumentation_WhenDocumentationIsForTerminal_ReturnsSolutionsDocumentations()
        {
            var controller = new DocumentationController(_activityMock.Object, _terminalMock.Object);
            var result = await controller.Activity(new ActivityDTO { Documentation = "Terminal=t" });
            Assert.IsTrue(result is OkNegotiatedContentResult<List<DocumentationResponseDTO>>, "Wrong result type is returned for terminal documentation type");
            _terminalMock.Verify(x => x.GetSolutionDocumentations("t"), Times.Once(), "Terminal documentation was not requested");
        }

        [Test]
        public async Task ActivityDocumentation_WhenDocumentationIsForMainPage_ReturnsSolutionDocumentation()
        {
            var controller = new DocumentationController(_activityMock.Object, _terminalMock.Object);
            var result = await controller.Activity(new ActivityDTO { Documentation = "MainPage" });
            Assert.IsTrue(result is OkNegotiatedContentResult<DocumentationResponseDTO>, "Wrong result type is returned for solution documentation type");
            _activityMock.Verify(x => x.GetActivityDocumentation<DocumentationResponseDTO>(It.IsAny<ActivityDTO>(), true), Times.Once(), "Solution documentation was not requested");
        }

        [Test]
        public async Task ActivityDocumentation_WhenDocumentationTypeIsIncorrect_ReturnsBadRequest()
        {
            var controller = new DocumentationController(_activityMock.Object, _terminalMock.Object);
            var result = await controller.Activity(new ActivityDTO { Documentation = "something" });
            Assert.IsTrue(result is BadRequestErrorMessageResult, "Wrong result type is returned for incorrect documentation type");
        }
    }
}
