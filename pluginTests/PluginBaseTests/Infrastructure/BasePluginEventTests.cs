using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.DataTransferObjects;
using Moq;
using NUnit.Framework;
using UtilitiesTesting;

namespace terminalTests.TerminalBaseTests.Infrastructure
{
    [TestFixture]
    [Category("BaseTerminalEvent")]
    public class BasePluginEventTests : BaseTest
    {
        private TestBaseTerminalEvent _baseTerminalEvent;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _baseTerminalEvent = new TestBaseTerminalEvent();
        }

        [Test]
        public void SendPluginErrorIncident_ShouldPostLoggingData_ToFr8EventController()
        {
            //Act
            _baseTerminalEvent.SendTerminalErrorIncident("something", "ex", "exception");

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(_baseTerminalEvent.RestfulServiceClient);

            //verify that the post call is made to Fr8 Event Controller
            restClientMock.Verify(
                client => client.PostAsync(new Uri(ConfigurationManager.AppSettings["EventWebServerUrl"], UriKind.Absolute), 
                    It.IsAny<CrateDTO>()), Times.Exactly(1));

            restClientMock.VerifyAll();
        }
    }
}
