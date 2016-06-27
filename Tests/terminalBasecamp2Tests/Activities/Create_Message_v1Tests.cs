using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.Testing.Unit;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalBasecamp2.Activities;
using terminalBasecamp2.Data;
using terminalBasecamp2.Infrastructure;

namespace terminalBasecamp2Tests.Activities
{
    [TestFixture]
    public class Create_Message_v1Tests : BaseTest
    {
        public override void SetUp()
        {
            base.SetUp();
            var basecampApiMock = new Mock<IBasecampApiClient>();
            ObjectFactory.Container.Inject(basecampApiMock);
            ObjectFactory.Container.Inject(basecampApiMock.Object);
            var hubCommunicatorMock = new Mock<IHubCommunicator>();
            ObjectFactory.Container.Inject(hubCommunicatorMock);
            ObjectFactory.Container.Inject(hubCommunicatorMock.Object);
        }

        private AuthorizationToken GetAuthorizationToken()
        {
            return new AuthorizationToken
            {
                Token = JsonConvert.SerializeObject(new BasecampAuthorizationToken())
            };
        }

        private ActivityContext GetActivityContext()
        {
            return new ActivityContext
            {
                HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = GetAuthorizationToken()
            };
        }

        private ContainerExecutionContext GetContainerContext()
        {
            return new ContainerExecutionContext
            {
                ContainerId = Guid.Empty,
                PayloadStorage = new CrateStorage(Crate.FromContent(string.Empty, new OperationalStateCM()))
            };
        }


        [Test]
        public async Task Initialize_Always_LoadsListOfAccounts()
        {
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            await activity.Configure(GetActivityContext());
            ObjectFactory.GetInstance<Mock<IBasecampApiClient>>().Verify(
                                                                         x => x.GetAccounts(It.IsAny<AuthorizationToken>()),
                                                                         Times.Once(),
                                                                         "Initial configuration didn't load list of accounts");
        }

        [Test]
        public async Task Initialize_Always_SignalsRuntimeCrateAndFields()
        {
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var activityContext = GetActivityContext();
            await activity.Configure(activityContext);
            var signalledCrate = activityContext.ActivityPayload.CrateStorage.FirstCrateOrDefault<CrateDescriptionCM>()?.Content;
            Assert.IsNotNull(signalledCrate, "Crate was not signalled");
            var signalledFields = signalledCrate.CrateDescriptions?.FirstOrDefault(x => x.ManifestId == (int)MT.StandardPayloadData);
            Assert.IsNotNull(signalledFields, "Fields were not signalled");
        }

        [Test]
        public async Task Initialize_WhenOnlyOneAccountExists_SelectsItAndHidesAccountSelector()
        {
            ConfigureSingleAccount();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            var activityUi = context.ActivityPayload.CrateStorage.GetReadonlyActivityUi<Create_Message_v1.ActivityUi>();
            Assert.IsNotEmpty(activityUi.AccountSelector.Value, "The only account was not selected");
            Assert.IsTrue(activityUi.AccountSelector.IsHidden, "Account selector was not hidden");
        }

        [Test]
        public async Task Initialize_WhenOnlyOneProjectExists_SelectsItAndHidesProjectSelector()
        {
            ConfigureSingleAccount();
            ConfigureSingleProject();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            var activityUi = context.ActivityPayload.CrateStorage.GetReadonlyActivityUi<Create_Message_v1.ActivityUi>();
            Assert.IsNotEmpty(activityUi.ProjectSelector.Value, "The only project was not selected");
            Assert.IsTrue(activityUi.ProjectSelector.IsHidden, "Project selector was not hidden");
        }

        [Test]
        public async Task Followup_WhenAccountIsSelected_LoadsProjects()
        {
            ConfigureMultipleAccounts();
            ConfigureMultipleProjects();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            context.ActivityPayload.CrateStorage.UpdateControls<Create_Message_v1.ActivityUi>(x => x.AccountSelector.SelectByValue(x.AccountSelector.ListItems[0].Value));
            await activity.Configure(context);
            ObjectFactory.GetInstance<Mock<IBasecampApiClient>>().Verify(x => x.GetProjects(It.IsAny<string>(), It.IsAny<AuthorizationToken>()),
                                                                         Times.Once(),
                                                                         "Projects were not loaded");
        }

        [Test]
        public async Task Run_Always_CreatesMessageAndMessageCrate()
        {
            ConfigureSingleAccount();
            ConfigureSingleProject();
            ConfigureMessageReturn();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var activityContext = GetActivityContext();
            var containerContext = GetContainerContext();
            await activity.Configure(activityContext);
            activityContext.ActivityPayload.CrateStorage.UpdateControls<Create_Message_v1.ActivityUi>(x =>
                                                                                              {
                                                                                                  x.MessageSubject.ValueSource = TextSource.SpecificValueSource;
                                                                                                  x.MessageSubject.TextValue = "Subject";
                                                                                                  x.MessageContent.ValueSource = TextSource.SpecificValueSource;
                                                                                                  x.MessageContent.TextValue = "Content";
                                                                                              });
            await activity.Configure(activityContext);
            await activity.Run(activityContext, containerContext);
            ObjectFactory.GetInstance<Mock<IBasecampApiClient>>().Verify(x => x.CreateMessage(It.IsAny<string>(), It.IsAny<string>(), "Subject", "Content", It.IsAny<AuthorizationToken>()),
                                                                         Times.Once(),
                                                                         "Message was not created");
            var messageCrate = containerContext.PayloadStorage.FirstCrateOrDefault<StandardPayloadDataCM>(x => x.Label == Create_Message_v1.RuntimeCrateLabel);
            Assert.IsNotNull(messageCrate, "Message create was not created");
        }

        [Test]
        public async Task Validate_WhenNoAccountExists_AddsErrorToValidationManager()
        {
            ConfigureNoAccounts();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            await activity.Activate(context);
            var validationManifest = context.ActivityPayload.CrateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();
            Assert.IsNotNull(validationManifest, "Validation errors crate doesn't exist in activity storage");
            Assert.AreEqual(1, validationManifest.GetErrorsForControl(nameof(Create_Message_v1.ActivityUi.AccountSelector)).Count, "Account selector doesn't have related error");
        }

        [Test]
        public async Task Validate_WhenAccountIsNotSelected_AddsErrorToValidationManager()
        {
            ConfigureMultipleAccounts();
            ConfigureNoProjects();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            await activity.Activate(context);
            var validationManifest = context.ActivityPayload.CrateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();
            Assert.IsNotNull(validationManifest, "Validation errors crate doesn't exist in activity storage");
            Assert.AreEqual(1, validationManifest.GetErrorsForControl(nameof(Create_Message_v1.ActivityUi.AccountSelector)).Count, "Account selector doesn't have related error");
        }

        [Test]
        public async Task Validate_WhenNoProjectExists_AddsErrorToValidationManager()
        {
            ConfigureSingleAccount();
            ConfigureNoProjects();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            await activity.Activate(context);
            var validationManifest = context.ActivityPayload.CrateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();
            Assert.IsNotNull(validationManifest, "Validation errors crate doesn't exist in activity storage");
            Assert.AreEqual(1, validationManifest.GetErrorsForControl(nameof(Create_Message_v1.ActivityUi.ProjectSelector)).Count, "Project selector doesn't have related error");
        }

        [Test]
        public async Task Validate_WhenProjectIsNotSelected_AddsErrorToValidationManager()
        {
            ConfigureSingleAccount();
            ConfigureMultipleProjects();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            await activity.Activate(context);
            var validationManifest = context.ActivityPayload.CrateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();
            Assert.IsNotNull(validationManifest, "Validation errors crate doesn't exist in activity storage");
            Assert.AreEqual(1, validationManifest.GetErrorsForControl(nameof(Create_Message_v1.ActivityUi.ProjectSelector)).Count, "Project selector doesn't have related error");
        }

        [Test]
        public async Task Validate_WhenSubjectOrContentIsNotSet_AddsErrorsToValidationManager()
        {
            ConfigureSingleAccount();
            ConfigureSingleProject();
            var activity = ObjectFactory.GetInstance<Create_Message_v1>();
            var context = GetActivityContext();
            await activity.Configure(context);
            await activity.Activate(context);
            var validationManifest = context.ActivityPayload.CrateStorage.CrateContentsOfType<ValidationResultsCM>().FirstOrDefault();
            Assert.IsNotNull(validationManifest, "Validation errors crate doesn't exist in activity storage");
            Assert.AreEqual(1, validationManifest.GetErrorsForControl(nameof(Create_Message_v1.ActivityUi.MessageSubject)).Count, "Message subject doesn't have related error");
            Assert.AreEqual(1, validationManifest.GetErrorsForControl(nameof(Create_Message_v1.ActivityUi.MessageContent)).Count, "Message content doesn't have related error");
        }

        private void ConfigureMessageReturn()
        {
            var baseApiClientMock = ObjectFactory.GetInstance<Mock<IBasecampApiClient>>();
            baseApiClientMock.Setup(x => x.CreateMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuthorizationToken>()))
                             .Returns<string, string, string, string, AuthorizationToken>((url, project, subject, content, token) => Task.FromResult(new Message { Id = 1, Subject = subject, Content = content }));
        }

        private void ConfigureNoProjects()
        {
            var baseApiClientMock = ObjectFactory.GetInstance<Mock<IBasecampApiClient>>();
            baseApiClientMock.Setup(x => x.GetProjects(It.IsAny<string>(), It.IsAny<AuthorizationToken>())).Returns(Task.FromResult(new List<Project>()));
        }

        private void ConfigureSingleProject()
        {
            var baseApiClientMock = ObjectFactory.GetInstance<Mock<IBasecampApiClient>>();
            baseApiClientMock.Setup(x => x.GetProjects(It.IsAny<string>(), It.IsAny<AuthorizationToken>())).Returns(Task.FromResult(new List<Project> { new Project { Name = "Name", Id = 1 } }));
        }

        private void ConfigureMultipleProjects()
        {
            var baseApiClientMock = ObjectFactory.GetInstance<Mock<IBasecampApiClient>>();
            baseApiClientMock.Setup(x => x.GetProjects(It.IsAny<string>(), It.IsAny<AuthorizationToken>())).Returns(Task.FromResult(new List<Project>
                                                                                                                                    {
                                                                                                                                        new Project { Name = "Name", Id = 1 },
                                                                                                                                        new Project { Name = "Name2", Id = 2 }
                                                                                                                                    }));
        }

        private void ConfigureNoAccounts()
        {
            var baseApiClientMock = ObjectFactory.GetInstance<Mock<IBasecampApiClient>>();
            baseApiClientMock.Setup(x => x.GetAccounts(It.IsAny<AuthorizationToken>())).Returns(Task.FromResult(new List<Account>()));
        }

        private static void ConfigureSingleAccount()
        {
            var baseApiClientMock = ObjectFactory.GetInstance<Mock<IBasecampApiClient>>();
            baseApiClientMock.Setup(x => x.GetAccounts(It.IsAny<AuthorizationToken>())).Returns(Task.FromResult(new List<Account> { new Account { Name = "Name", ApiUrl = "url" } }));
        }

        private static void ConfigureMultipleAccounts()
        {
            var baseApiClientMock = ObjectFactory.GetInstance<Mock<IBasecampApiClient>>();
            baseApiClientMock.Setup(x => x.GetAccounts(It.IsAny<AuthorizationToken>())).Returns(Task.FromResult(new List<Account>
                                                                                                                {
                                                                                                                    new Account { Name = "Name", ApiUrl = "url" },
                                                                                                                    new Account { Name = "Name1", ApiUrl = "url1" }
                                                                                                                }));
        }
    }
}
