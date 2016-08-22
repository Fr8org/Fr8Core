//using System;
//using System.IO;
//using System.Net.Http;
//using System.Text;
//using Core.Interfaces;
//using Moq;
//using NUnit.Framework;
//using Fr8.Testing.Unit;
//using Fr8.Testing.Unit.Fixtures;
//using Web.Controllers;

//namespace HubTests.Controllers
//{
//    [ TestFixture ]
	
//    [ Category( "Controllers.Api.Notification" ) ]
//    public class NotificationControllerTests: BaseTest
//    {
//        private string _xmlPayloadFullPath;

//        private string _testUserId = "testuser";

//        [ SetUp ]
//        public override void SetUp()
//        {
//            this._xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath( Environment.CurrentDirectory );
//            if( this._xmlPayloadFullPath == string.Empty )
//                throw new Exception( "XML payload file for testing DocuSign notification is not found. Env path: " + Environment.CurrentDirectory );
//            base.SetUp();
//        }

//        [ Test ]
//        public void NotificationController_CanHandleNotification()
//        {
//            //Arrange 
//            var mockProcess = new Mock< IDocuSignNotification >();
//            mockProcess.Setup( e => e.Process( It.IsAny< string >(), It.IsAny< string >() ) );

//            var notificationController = new DocuSignNotificationController( mockProcess.Object );
//            var xmlPayload = File.ReadAllText( this._xmlPayloadFullPath );
//            var request = new HttpRequestMessage( HttpMethod.Post, "localhost" );
//            request.Content = new StringContent( xmlPayload, Encoding.UTF8, "text/xml" );
//            notificationController.Request = request;

//            //Act
//            notificationController.Post( this._testUserId ).Wait();

//            //Assert
//            mockProcess.Verify( e => e.Process( this._testUserId, xmlPayload ) );
//        }
//    }
//}