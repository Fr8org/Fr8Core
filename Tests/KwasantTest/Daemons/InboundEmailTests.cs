using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.ExternalServices;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class InboundEmailTests : BaseTest
    {
        [Test]
        public void TestInboundEmail()
        {
            // SETUP
            const string testFromEmailAddress = "test.user@gmail.com";
            const string testSubject = "Test Subject";
            const string testBody = "Test Body";
            const string testToEmailAddress = "test.recipient@gmail.com";

            var mailMessage = new MailMessage();

            mailMessage.Body = testBody;
            mailMessage.Subject = testSubject;
            mailMessage.From = new MailAddress(testFromEmailAddress);
            mailMessage.To.Add(new MailAddress(testToEmailAddress));

            var clientMock = new Mock<IImapClient>();

            clientMock.Setup(c => c.GetMessages(It.IsAny<IEnumerable<uint>>(), true, null))
                .Returns(new List<MailMessage> { mailMessage });

            var imapClient = clientMock.Object;

            ObjectFactory.Configure(a => a.For<IImapClient>().Use(imapClient));
            
            var ie = new InboundEmail();
            DaemonTests.RunDaemonOnce(ie);
            
            // VERIFY
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestRepo = uow.BookingRequestRepository;
                var bookingRequests = bookingRequestRepo.GetAll().ToList();

                Assert.AreEqual(1, bookingRequests.Count);
                var bookingRequest = bookingRequests.First();
                Assert.AreEqual(testFromEmailAddress, bookingRequest.From.Address);
                Assert.AreEqual(testSubject, bookingRequest.Subject);
                Assert.AreEqual(testBody, bookingRequest.HTMLText);
                Assert.AreEqual(testFromEmailAddress, bookingRequest.Customer.EmailAddress.Address);
                Assert.AreEqual(1, bookingRequest.To.Count());
                Assert.AreEqual(testToEmailAddress, bookingRequest.To.First().Address);
            }
        }

        [Test]
        public void CanProcessAttachments()
        {
            // SETUP
            const string testFromEmailAddress = "test.user@gmail.com";
            const string testSubject = "Test Subject";
            const string testBody = "Test Body";
            const string testToEmailAddress = "test.recipient@gmail.com";

            var mailMessage = new MailMessage();

            mailMessage.Body = testBody;
            mailMessage.Subject = testSubject;
            mailMessage.From = new MailAddress(testFromEmailAddress);
            mailMessage.To.Add(new MailAddress(testToEmailAddress));

            var memStr = new MemoryStream();
            var data = new byte[] {1, 2, 3};
            memStr.Write(data, 0, data.Length);
            mailMessage.AlternateViews.Add(new AlternateView(memStr, "image/png"));

            var clientMock = new Mock<IImapClient>();

            clientMock.Setup(c => c.GetMessages(It.IsAny<IEnumerable<uint>>(), true, null))
                .Returns(new List<MailMessage> { mailMessage });

            var imapClient = clientMock.Object;

            ObjectFactory.Configure(a => a.For<IImapClient>().Use(imapClient));

            var ie = new InboundEmail();
            DaemonTests.RunDaemonOnce(ie);

            // VERIFY
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestRepo = uow.BookingRequestRepository;
                var bookingRequests = bookingRequestRepo.GetAll().ToList();

                Assert.AreEqual(1, bookingRequests.Count);
                var bookingRequest = bookingRequests.First();
                Assert.AreEqual(testFromEmailAddress, bookingRequest.From.Address);
                Assert.AreEqual(testSubject, bookingRequest.Subject);
                Assert.AreEqual(testBody, bookingRequest.HTMLText);
                Assert.AreEqual(testFromEmailAddress, bookingRequest.Customer.EmailAddress.Address);
                Assert.AreEqual(1, bookingRequest.To.Count());
                Assert.AreEqual(testToEmailAddress, bookingRequest.To.First().Address);
                Assert.AreEqual(1, bookingRequest.Attachments.Count());
            }
        }

        [Test]
        public void CanProcessInvitationResponse()
        {
            // SETUP
            AttendeeDO curAttendee;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var curUser = fixture.TestUser1();
                var curEvent = fixture.TestEvent2();
                curAttendee = curEvent.Attendees[0];

                uow.UserRepository.Add(curUser);
                uow.EventRepository.Add(curEvent);
                uow.SaveChanges();

                var testInvitationResponseIcs = string.Format(
                    @"BEGIN:VCALENDAR
METHOD:REPLY
PRODID:Microsoft Exchange Server 2010
VERSION:2.0
BEGIN:VTIMEZONE
TZID:Pacific Standard Time
BEGIN:STANDARD
DTSTART:16010101T020000
TZOFFSETFROM:-0700
TZOFFSETTO:-0800
RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=1SU;BYMONTH=11
END:STANDARD
BEGIN:DAYLIGHT
DTSTART:16010101T020000
TZOFFSETFROM:-0800
TZOFFSETTO:-0700
RRULE:FREQ=YEARLY;INTERVAL=1;BYDAY=2SU;BYMONTH=3
END:DAYLIGHT
END:VTIMEZONE
BEGIN:VEVENT
ATTENDEE;PARTSTAT={0};CN=Vik Mehta:MAILTO:{1}
COMMENT;LANGUAGE=en-US:\n
SUMMARY;LANGUAGE=en-US:Accepted: Invitation from: Alex Edelstein - Meet for Cofee: Alex <-> Vik - 7/29/2014 1:00:00 PM -07:00
DTSTART;TZID=Pacific Standard Time:20140729T130000
DTEND;TZID=Pacific Standard Time:20140729T140000
UID:{2}
CLASS:PUBLIC
PRIORITY:5
DTSTAMP:20140723T172604Z
TRANSP:OPAQUE
STATUS:CONFIRMED
SEQUENCE:0
LOCATION;LANGUAGE=en-US:Starbucks Corporation 176 Gateway Blvd South San Francisco, CA 94080
X-MICROSOFT-CDO-APPT-SEQUENCE:0
X-MICROSOFT-CDO-BUSYSTATUS:BUSY
X-MICROSOFT-CDO-INTENDEDSTATUS:BUSY
X-MICROSOFT-CDO-ALLDAYEVENT:FALSE
X-MICROSOFT-CDO-IMPORTANCE:1
X-MICROSOFT-CDO-INSTTYPE:0
X-MICROSOFT-DISALLOW-COUNTER:FALSE
END:VEVENT
END:VCALENDAR",
                    KwasantICS.DDay.iCal.ParticipationStatus.Accepted, curAttendee.EmailAddress.Address,
                    curEvent.ExternalGUID);

                var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes(testInvitationResponseIcs));

                var mailMessage = new MailMessage();

                mailMessage.AlternateViews.Add(new AlternateView(attachmentStream, "text/calendar"));
                mailMessage.From = new MailAddress(curAttendee.EmailAddress.Address);

                var clientMock = new Mock<IImapClient>();

                clientMock.Setup(c => c.GetMessages(It.IsAny<IEnumerable<uint>>(), true, null))
                    .Returns(new List<MailMessage> { mailMessage });

                var imapClient = clientMock.Object;

                ObjectFactory.Configure(a => a.For<IImapClient>().Use(imapClient));

                uow.SaveChanges();
            }

            var ie = new InboundEmail();

            // EXECUTE
            DaemonTests.RunDaemonOnce(ie);

            // VERIFY
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var invitationResponses = uow.InvitationResponseRepository.GetAll().ToList();

                Assert.AreEqual(1, invitationResponses.Count);
                var curInvitationResponse = invitationResponses.First();
                Assert.AreEqual(curAttendee.EmailAddress.Address, curInvitationResponse.From.Address);
                Assert.AreEqual(curAttendee.EmailAddress.Address, curInvitationResponse.Attendee.EmailAddress.Address);
                Assert.AreEqual(Data.States.ParticipationStatus.Accepted, curInvitationResponse.Attendee.ParticipationStatus);
            }
        }
    }
}
