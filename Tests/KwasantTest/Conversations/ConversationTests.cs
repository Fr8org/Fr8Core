using System.Collections.Generic;
using System.Net.Mail;
using Data.Interfaces;
using Data.States;
using KwasantCore.Services;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Conversations
{
    public class ConversationTests : BaseTest
    {
        [Test]
        public void TestExactSubjectMatchesConversation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(bookingRequestDO);
                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, null, bookingRequestDO.Subject, bookingRequestDO.From.Address);
                Assert.AreEqual(bookingRequestDO, matchedBookingRequest);
            }
        }

        [Test]
        public void TestRESubjectMatchesConversation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(bookingRequestDO);
                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, null, "RE: " + bookingRequestDO.Subject, bookingRequestDO.From.Address);
                Assert.AreEqual(bookingRequestDO, matchedBookingRequest);
            }
        }

        [Test]
        public void TestNonExactSubjectDoesNotMatchConversation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(bookingRequestDO);
                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, null, bookingRequestDO.Subject + "THIS IS A DIFFERENT SUBJECT", bookingRequestDO.From.Address);
                Assert.Null(matchedBookingRequest);
            }
        }

        [Test]
        public void TestInvalidBookingRequestDoesNotMatchConversation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                bookingRequestDO.State = BookingRequestState.Invalid;
                uow.BookingRequestRepository.Add(bookingRequestDO);
                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, null, bookingRequestDO.Subject, bookingRequestDO.From.Address);
                Assert.Null(matchedBookingRequest);
            }
        }

        [Test]
        public void TestFinishedBookingRequestDoesNotMatchConversation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                bookingRequestDO.State = BookingRequestState.Finished;
                uow.BookingRequestRepository.Add(bookingRequestDO);
                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, null, bookingRequestDO.Subject, bookingRequestDO.From.Address);
                Assert.Null(matchedBookingRequest);
            }
        }

        [Test]
        public void TestDifferentRecipientDoesNotMatchConversation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(bookingRequestDO);
                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, null, bookingRequestDO.Subject, bookingRequestDO.From.Address + ".xyz");
                Assert.Null(matchedBookingRequest);
            }
        }

        [Test]
        public void TestRecipientInConversationMatchesConversation()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(bookingRequestDO);

                var emailDO = fixture.TestEmail3();
                emailDO.Conversation = bookingRequestDO;

                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, null, bookingRequestDO.Subject, emailDO.From.Address);
                Assert.AreEqual(bookingRequestDO, matchedBookingRequest);
            }
        }

        [Test]
        public void TestMatchingByMessageID()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(bookingRequestDO);

                var emailDO = fixture.TestEmail3();
                emailDO.Conversation = bookingRequestDO;

                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, new Dictionary<string, string> { { "References", emailDO.MessageID }}, null, "DIFFERENT SUBJECT", emailDO.From.Address + ".DIFFERENTEMAIL");
                Assert.AreEqual(bookingRequestDO, matchedBookingRequest);
            }
        }

        [Test]
        public void TestMatchingByReplyTo()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);
                var bookingRequestDO = fixture.TestBookingRequest1();
                uow.BookingRequestRepository.Add(bookingRequestDO);

                var emailDO = fixture.TestEmail3();
                emailDO.Conversation = bookingRequestDO;

                uow.SaveChanges();

                var matchedBookingRequest = Conversation.Match(uow, null, new[] { new MailAddress("test+" + bookingRequestDO.Id + "@sant.com"), }, "DIFFERENT SUBJECT", emailDO.From.Address + ".DIFFERENTEMAIL");
                Assert.AreEqual(bookingRequestDO, matchedBookingRequest);
            }
        }
    }
}
