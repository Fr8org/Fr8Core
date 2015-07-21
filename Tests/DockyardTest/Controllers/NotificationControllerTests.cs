using System;
using NUnit.Framework;
using Microsoft.Owin.Hosting;
using System.Net.Http;
using System.IO;
using System.Text;
using System.Net;
using Web.Controllers;
using StructureMap;
using Moq;
using Core.Interfaces;

namespace DockyardTest.Controllers
{
    [TestFixture]
    public class NotificationControllerTests : BaseTest
    {

        string _baseAddress = "http://localhost:9124";
        string _testUserId = "testuser";

        [SetUp]
        public override void SetUp()
        {
            WebApp.Start<WebServer>(url: _baseAddress);
            base.SetUp();
        }

        [Test]
        [Category("Api")]
        public void NotificationController_HandleDocusignNotification_MustReturnHttpStatus200()
        {
            string xmlPayLoadLocation = "../../Content/DocusignXmlPayload.xml";
            string address = _baseAddress + "/Notification?userID=" + _testUserId;

            HttpResponseMessage response = MakeRequest(
                address, 
                File.ReadAllText(xmlPayLoadLocation), 
                "text/xml", 
                HttpMethod.Post);
            string text = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [Category("Api")]
        public void NotificationController_CanHandleNotification()
        {
            //Arrange 
            var moqProcess = new Mock<IProcess>();
            moqProcess.Setup(e => e.HandleDocusignNotification(It.IsAny<String>(), It.IsAny<String>()));
            NotificationController notificationController = new NotificationController(moqProcess.Object);
            string xmlPayloadLocation = "../../Content/DocusignXmlPayload.xml";
            string xmlPayload = File.ReadAllText(xmlPayloadLocation);
            var request = new HttpRequestMessage(HttpMethod.Post, _baseAddress);
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
