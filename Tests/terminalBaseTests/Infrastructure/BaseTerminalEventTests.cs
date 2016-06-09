using System;
using Moq;
using NUnit.Framework;
using UtilitiesTesting;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;
using StructureMap;

namespace terminalBaseTests.Infrastructure
{
    [TestFixture]
    [Category("BaseTerminalEvent")]
    public class BaseTerminalEventTests : BaseTest
    {
        private BaseTerminalEvent _baseTerminalEvent;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void SendTerminalErrorIncident_ShouldPostLoggingData_ToFr8EventController()
        {
            var restClientMock = new Mock<IRestfulServiceClient>(MockBehavior.Default);
            _baseTerminalEvent = new BaseTerminalEvent(restClientMock.Object);
            _baseTerminalEvent.SendTerminalErrorIncident("something", "ex", "exception","test-id");
            
            //verify that the post call is made to Fr8 Event Controller
            restClientMock.Verify(
                client => client.PostAsync(new Uri("http://localhost:30643/api/v1/events", UriKind.Absolute), 
                    It.IsAny<CrateDTO>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Exactly(1));

            restClientMock.VerifyAll();
        }
    }
}
