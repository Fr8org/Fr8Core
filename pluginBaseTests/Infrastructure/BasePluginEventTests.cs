using System;
using Data.Crates;
using Moq;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;
using UtilitiesTesting;
using Utilities.Configuration.Azure;

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
                    It.IsAny<CrateSerializationProxy>()), Times.Exactly(1));

            restClientMock.VerifyAll();
        }
    }
}
