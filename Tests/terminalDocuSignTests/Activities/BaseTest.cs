using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Crates;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using Fr8.Testing.Unit.Fixtures;
using TerminalBase.BaseClasses;
using Fr8Data.Manifests;
using Fr8Data.Managers;
using TerminalBase.Models;

namespace terminalDocuSignTests.Activities
{
    public class BaseTest : Fr8.Testing.Unit.BaseTest
    {
        protected readonly AuthorizationToken FakeToken = new AuthorizationToken { Token = "1" };
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            hubCommunicatorMock.Setup(x => x.GetPayload(It.IsAny<Guid>()))
                               .Returns(Task.FromResult(FixtureData.PayloadDTO1()));
            ObjectFactory.Configure(x => x.For<Mock<IHubCommunicator>>().Use(hubCommunicatorMock));
            ObjectFactory.Configure(x => x.For<IHubCommunicator>().Use(hubCommunicatorMock.Object));
            var docuSignPlanMock = new Mock<IDocuSignPlan>();
            ObjectFactory.Configure(x => x.For<Mock<IDocuSignPlan>>().Use(docuSignPlanMock));
            ObjectFactory.Configure(x => x.For<IDocuSignPlan>().Use(docuSignPlanMock.Object));
            var docuSignManagerMock = new Mock<IDocuSignManager>();
            ObjectFactory.Configure(x => x.For<Mock<IDocuSignManager>>().Use(docuSignManagerMock));
            ObjectFactory.Configure(x => x.For<IDocuSignManager>().Use(docuSignManagerMock.Object));
            var docuSignFoldersMock = new Mock<IDocuSignFolders>();
            ObjectFactory.Configure(x => x.For<Mock<IDocuSignFolders>>().Use(docuSignFoldersMock));
            ObjectFactory.Configure(x => x.For<IDocuSignFolders>().Use(docuSignFoldersMock.Object));
        }

        protected async Task<ValidationResultsCM> Validate(BaseTerminalActivity activity, ActivityContext activityContext)
        {
            var activityPayload = activityContext.ActivityPayload;
            activityPayload.CrateStorage.Remove<ValidationResultsCM>();

            var currentValidationResults = activityPayload.CrateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();

            if (currentValidationResults == null)
            {
                currentValidationResults = new ValidationResultsCM();
                activityPayload.CrateStorage.Add(Crate.FromContent("Validation Results", currentValidationResults));
            }

            //var validationManager = new ValidationManager(currentValidationResults, null);
            //let's trigger validation by calling activate
            await activity.Activate(activityContext);

            return activityPayload.CrateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();
        }

        protected void AssertErrorMessage(ValidationResultsCM validationResults, string errorMessage)
        {
            Assert.IsTrue(validationResults.ValidationErrors.Any(x => x.ErrorMessage == errorMessage), $"Error message '{errorMessage}' is missing");
        }
    }
}
