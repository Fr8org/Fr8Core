using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.Manifests;
using Fr8Data.Crates;
using Hub.Managers;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using UtilitiesTesting.Fixtures;
using TerminalBase.BaseClasses;

namespace terminalDocuSignTests.Activities
{
    public class BaseTest : UtilitiesTesting.BaseTest
    {
        protected readonly AuthorizationTokenDO FakeToken = new AuthorizationTokenDO { Token = "1" };
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            hubCommunicatorMock.Setup(x => x.GetPayload(It.IsAny<ActivityDO>(), It.IsAny<Guid>(), It.IsAny<string>()))
                               .Returns(Task.FromResult(FixtureData.PayloadDTO2()));
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

        protected async Task<ValidationResultsCM> Validate(BaseTerminalActivity activity, ActivityDO activityDo)
        {
            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(activityDo))
            {
                crateStorage.Remove<ValidationResultsCM>();

                var currentValidationResults = crateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();

                if (currentValidationResults == null)
                {
                    currentValidationResults = new ValidationResultsCM();
                    crateStorage.Add(Crate.FromContent("Validation Results", currentValidationResults));
                }

                var validationManager = new ValidationManager(currentValidationResults);

                await activity.ValidateActivity(activityDo, crateStorage, validationManager);

                return currentValidationResults;
            }
        }

        protected void AssertControlErrorMessage(ValidationResultsCM validationResults, string controlName, string errorMessage)
        {
            var errors = validationResults.GetErrorsForControl(controlName);

            Assert.IsTrue(errors.Any(x => x == errorMessage), "Error message is missing for control: " + controlName);
        }

        protected void AssertErrorMessage(ValidationResultsCM validationResults, string errorMessage)
        {
            Assert.IsTrue(validationResults.ValidationErrors.Any(x => x.ErrorMessage == errorMessage), $"Error message '{errorMessage}' is missing");
        }
    }
}
