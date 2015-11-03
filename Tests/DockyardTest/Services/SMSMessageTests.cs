using Moq;
using NUnit.Framework;
using StructureMap;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Packagers;
using UtilitiesTesting;

namespace DockyardTest.Services
{
//    [TestFixture]
//    [Category("SMSMessage")]
//    public class SMSMessageTests : BaseTest
//    {
//        private ISMSMessage _smsMessage;
//
//        [SetUp]
//        public override void SetUp()
//        {
//            base.SetUp();
//
//            Mock<ISMSPackager> smsPackager = new Mock<ISMSPackager>(MockBehavior.Default);
//            ObjectFactory.Container.Inject(typeof(ISMSPackager), smsPackager.Object);
//
//            _smsMessage = ObjectFactory.GetInstance<ISMSMessage>();
//        }
//
//        [Test]
//        public void SMSMessage_Send_ShouldCall_SmsPackagerSendMethodOnce()
//        {
//            //Arrange
//            var ss = Mock.Get(ObjectFactory.GetInstance<ISMSPackager>());
//            
//            //Act
//            _smsMessage.Send(string.Empty, string.Empty);
//
//            //Assert
//            ss.Verify(p => p.SendSMS(string.Empty, string.Empty), Times.Exactly(1));
//            ss.VerifyAll();
//        }
//    }
}
