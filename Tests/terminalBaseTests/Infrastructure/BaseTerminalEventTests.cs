using System;
using Moq;
using NUnit.Framework;
using UtilitiesTesting;
using Utilities.Configuration.Azure;
using System.Collections.Generic;
using Fr8Data.DataTransferObjects;
using Fr8Infrastructure.Interfaces;

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
            _baseTerminalEvent.SendTerminalErrorIncident("something", "ex", "exception","test-id");

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(_baseTerminalEvent.RestfulServiceClient);

            //verify that the post call is made to Fr8 Event Controller
            restClientMock.Verify(
                client => client.PostAsync(new Uri("http://localhost:30643/api/v1/events", UriKind.Absolute), 
                    It.IsAny<CrateDTO>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Exactly(1));

            restClientMock.VerifyAll();
        }
    }
}
