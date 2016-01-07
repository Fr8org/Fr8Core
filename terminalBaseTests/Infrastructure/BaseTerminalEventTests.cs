using System;
using Data.Crates;
using Moq;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;
using UtilitiesTesting;
using Utilities.Configuration.Azure;

namespace terminalBaseTests.Infrastructure
{
    [TestFixture]
    [Category("BaseTerminalEvent")]
    public class BaseTerminalEventTests : BaseTest
    {
        private TestBaseTerminalEvent _baseTerminalEvent;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _baseTerminalEvent = new TestBaseTerminalEvent();
        }

        [Test]
        public void SendTerminalErrorIncident_ShouldPostLoggingData_ToFr8EventController()
        {
            //Act
            _baseTerminalEvent.SendTerminalErrorIncident("something", "ex", "exception");

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(_baseTerminalEvent.RestfulServiceClient);

            //verify that the post call is made to Fr8 Event Controller
            restClientMock.Verify(
                client => client.PostAsync(new Uri(CloudConfigurationManager.GetSetting("CoreWebServerUrl") + "api/v1/event/gen1_event", UriKind.Absolute), 
                    It.IsAny<CrateDTO>(), It.IsAny<string>()), Times.Exactly(1));

            restClientMock.VerifyAll();
        }
    }
}
