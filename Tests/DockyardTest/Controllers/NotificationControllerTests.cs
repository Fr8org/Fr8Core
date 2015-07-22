using System;
using NUnit.Framework;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Net;
using Web.Controllers;
using StructureMap;
using Moq;
using Core.Interfaces;
using DockyardTest.Fixtures;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Ignore("Tests do not pass on CI.")]
    [Category("Api")]

    public class NotificationControllerTests : BaseTest
    {
        private string _xmlPayloadFullPath;

        string _testUserId = "testuser";

        [SetUp]
        public override void SetUp()
        {
            _xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory);
            if (_xmlPayloadFullPath == string.Empty)
                throw new Exception("XML payload file for testing DocuSign notification is not found. Env path: " + Environment.CurrentDirectory);
            base.SetUp();
        }

        [Test]
        public void NotificationController_CanHandleNotification()
        {
            //Arrange 
            var mockProcess = new Mock<IProcessService>();
            mockProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));

            NotificationController notificationController =new NotificationController(mockProcess.Object);
            string xmlPayload = File.ReadAllText(_xmlPayloadFullPath);
            var request = new HttpRequestMessage(HttpMethod.Post, "localhost");
            request.Content = new StringContent(xmlPayload, Encoding.UTF8, "text/xml");
            notificationController.Request = request;

            //Act
            notificationController.HandleDocusignNotification(_testUserId).Wait();

            //Assert
            mockProcess.Verify(e => e.HandleDocusignNotification(_testUserId, xmlPayload));
        }
    }
}
