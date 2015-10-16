using System;
using Core.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.DataTransferObjects;
using Moq;
using NUnit.Framework;
using fr8.Microsoft.Azure;
using UtilitiesTesting;

namespace pluginBaseTests.Infrastructure
{
    [TestFixture]
    [Category("BasePluginEvent")]
    public class BasePluginEventTests : BaseTest
    {
        private TestBasePluginEvent _basePluginEvent;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginEvent = new TestBasePluginEvent();
        }

        [Test]
        public void SendPluginErrorIncident_ShouldPostLoggingData_ToFr8EventController()
        {
            //Act
            _basePluginEvent.SendPluginErrorIncident("something", "ex", "exception");

            //Assert
            Mock<IRestfulServiceClient> restClientMock = Mock.Get(_basePluginEvent.RestfulServiceClient);

            //verify that the post call is made to Fr8 Event Controller
            restClientMock.Verify(
                client => client.PostAsync(new Uri(CloudConfigurationManager.GetSetting("EventWebServerUrl"), UriKind.Absolute), 
                    It.IsAny<CrateDTO>()), Times.Exactly(1));

            restClientMock.VerifyAll();
        }
    }
}
