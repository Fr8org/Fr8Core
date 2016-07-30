using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.StructureMap;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Models;
using Moq;
using NUnit.Framework;
using StructureMap;
using terminalPapertrail.Actions;
using terminalPapertrail.Interfaces;
using terminalPapertrail.Tests.Infrastructure;
using Fr8.Testing.Unit;

namespace terminalPapertrail.Tests.Actions
{
    [Ignore]
    [TestFixture]
    [Category("terminalPapertrailActions")]
    public class Write_To_LogTests : BaseTest
    {
        private Write_To_Log_v1 _activity_under_test;

        public override void SetUp()
        {
            base.SetUp();
            TerminalBootstrapper.ConfigureTest();
            TerminalPapertrailMapBootstrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            AutoMapperBootstrapper.ConfigureAutoMapper();
            _activity_under_test = New<Write_To_Log_v1>();
        }

        [Test]
        public async Task Configure_InitialConfigurationResponse_ShourldReturn_OneConfigControlsCrate()
        {
            //Act
            var testAction = new ActivityContext()
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = null,
                UserId = null,
            };
            await _activity_under_test.Configure(testAction);
            ActivityDTO resultActionDTO = Mapper.Map<ActivityDTO>(testAction.ActivityPayload);

            //Assert
            var crateStorage = new CrateManager().FromDto(resultActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count, "Initial configuration is failed for Write To Log activity in Papertrail");

            var configControlCrates = crateStorage.CratesOfType<StandardConfigurationControlsCM>().ToList();
            Assert.AreEqual(1, configControlCrates.Count, "More than one configuration controls are avaialbe for Write To Log activity");

            var targetUrlControl = configControlCrates.First().Content.Controls[0];
            Assert.IsNotNull(targetUrlControl, "Papertrail target URL control is not configured.");
            Assert.AreEqual("TargetUrlTextBox", targetUrlControl.Name, "Papertrail target URL control is not configured correctly");
        }

        [Test]
        public async Task Configure_FollowUpConfigurationResponse_ShourldReturn_OneConfigControlsCrate()
        {
            //Arrange
            var testAction = new ActivityContext()
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                //HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                AuthorizationToken = null,
                UserId = null
            };
            await _activity_under_test.Configure(testAction);

            //Act
            await _activity_under_test.Configure(testAction);
            ActivityDTO resultActionDTO = Mapper.Map<ActivityDTO>(testAction.ActivityPayload);

            //Assert
            var crateStorage = new CrateManager().FromDto(resultActionDTO.CrateStorage);
            Assert.AreEqual(1, crateStorage.Count, "Followup configuration is failed for Write To Log activity in Papertrail");

            var configControlCrates = crateStorage.CratesOfType<StandardConfigurationControlsCM>().ToList();
            Assert.AreEqual(1, configControlCrates.Count, "More than one configuration controls are avaialbe for Write To Log activity");

            var targetUrlControl = configControlCrates.First().Content.Controls[0];
            Assert.IsNotNull(targetUrlControl, "Papertrail target URL control is not configured.");
            Assert.AreEqual("TargetUrlTextBox", targetUrlControl.Name, "Papertrail target URL control is not configured correctly");
        }


        [Test]
        public async Task Run_WithOneUpstreamLogMessage_ShouldLogOneTime()
        {
            //Arrange

            //create initial and follow up configuration
            var testAction = new ActivityContext()
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = null,
                UserId = null
            };
            var executionContext = new ContainerExecutionContext();

            executionContext.PayloadStorage = new CrateStorage(Crate.FromContent("", new OperationalStateCM()));

            executionContext.PayloadStorage.Add("Log Messages", new StandardLoggingCM
            {
                Item = new List<LogItemDTO>()
                {
                    new LogItemDTO() {Activity = "A", Data = "Test Log Message"}
                }
            });

            await _activity_under_test.Configure(testAction);
            await _activity_under_test.Configure(testAction);

            //Act
            await _activity_under_test.Run(testAction, executionContext);

            //Assert
            var loggedMessge = executionContext.PayloadStorage.CrateContentsOfType<StandardLoggingCM>().Single();
            Assert.IsNotNull(loggedMessge, "Logged message is missing from the payload");
            Assert.AreEqual(1, loggedMessge.Item.Count, "Logged message is missing from the payload");

            Assert.IsTrue(loggedMessge.Item[0].IsLogged, "Log did not happen");

            Mock<IPapertrailLogger> papertrailLogger = Mock.Get(ObjectFactory.GetInstance<IPapertrailLogger>());
            papertrailLogger.Verify(logger => logger.LogToPapertrail(It.IsAny<string>(), It.IsAny<int>(), "Test Log Message"), Times.Exactly(1));
            papertrailLogger.VerifyAll();
        }

        [Test]
        public async Task Run_SecondTimeForSameLog_WithOneUpstreamLogedMessage_LoggedAlready_ShouldLogOnlyOneTime()
        {
            //Arrange

            //create initial and follow up configuration
            var testAction = new ActivityContext()
            {
                ActivityPayload = new ActivityPayload
                {
                    CrateStorage = new CrateStorage()
                },
                AuthorizationToken = null,
                UserId = null
            };
            await _activity_under_test.Configure(testAction);
            await _activity_under_test.Configure(testAction);

            var executionContext = new ContainerExecutionContext();
            executionContext.PayloadStorage = new CrateStorage(Crate.FromContent("", new OperationalStateCM()));

            executionContext.PayloadStorage.Add("Log Messages", new StandardLoggingCM
            {
                Item = new List<LogItemDTO>()
                {
                    new LogItemDTO() {Activity = "A", Data = "Test Log Message"}
                }
            });

            //log first time
            await _activity_under_test.Run(testAction, executionContext);

            //Act
            //try to log the same message again
            await _activity_under_test.Run(testAction, executionContext);

            //Assert
            var loggedMessge = executionContext.PayloadStorage.CrateContentsOfType<StandardLoggingCM>().Single();
            Assert.IsNotNull(loggedMessge, "Logged message is missing from the payload");
            Assert.AreEqual(1, loggedMessge.Item.Count, "Logged message is missing from the payload");

            Assert.IsTrue(loggedMessge.Item[0].IsLogged, "Log did not happen");

            Mock<IPapertrailLogger> papertrailLogger = Mock.Get(ObjectFactory.GetInstance<IPapertrailLogger>());
            papertrailLogger.Verify(logger => logger.LogToPapertrail(It.IsAny<string>(), It.IsAny<int>(), "Test Log Message"), Times.Exactly(1));
            papertrailLogger.VerifyAll();
        }
    }
}