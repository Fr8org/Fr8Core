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
    public class NotificationControllerTests : BaseTest 
    {
        private string _xmlPayloadFullPath;

        string _testUserId = "testuser";

        [SetUp]
        public override void SetUp()
        {
            _xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory);
            if (_xmlPayloadFullPath == string.Empty)
                throw new Exception("XML payload file for testing DocuSign notification is not found.");
            base.SetUp();
        }

        [Test]
        [Category("Api")]
        public void NotificationController_CanHandleNotification()
        {
            //Arrange 
            var moqProcess = new Mock<IProcess>();
            moqProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
            NotificationController notificationController = new NotificationController(moqProcess.Object);
            string xmlPayload = File.ReadAllText(_xmlPayloadFullPath);
            var request = new HttpRequestMessage(HttpMethod.Post, "localhost");
            request.Content = new StringContent(xmlPayload, Encoding.UTF8, "text/xml");
            notificationController.Request = request;

            //Act
            notificationController.HandleDocusignNotification(_testUserId).Wait();

            //Assert
            moqProcess.Verify(e => e.HandleDocusignNotification(_testUserId, xmlPayload));
        }


        private HttpResponseMessage MakeRequest(string url, string data, string mimeType, HttpMethod method)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                {
                    if (data != null)
                    {
                        request.Content = new StringContent(data, Encoding.UTF8, mimeType);
                    }
                    return client.SendAsync(request).Result;
                }
            }
        }
    }
}
