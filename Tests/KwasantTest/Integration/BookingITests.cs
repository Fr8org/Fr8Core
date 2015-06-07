using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.ExternalServices;
using KwasantCore.Services;
using KwasantTest.Daemons;
using KwasantWeb.Controllers;
using KwasantWeb.ViewModels;
using Moq;
using NUnit.Framework;
using SendGrid;
using StructureMap;

namespace KwasantTest.Integration
{
    [TestFixture]
    public class BookingITests : BaseTest
    {
        [Test]
        [Category("IntegrationTests")]
        public void ITest_CanProcessBRCreateEventAndSendInvite()
        {
            //The test is setup like this:
            //1. We sent an email which is to be turned into a booking request. This simulates a customer sending us an email
            //2. We confirm the booking request is created for the sent email
            //3. We create an event for that booking request
            //4. We call 'dispatch invitations' on the new event
            //5. We check that each attendee recieves an invitation

            //This lets us be sure we created the booking request for the right email
            string uniqueCustomerEmailSubject = Guid.NewGuid().ToString();
            
            //This stores the emails which we have sent, but not yet read. It lets us pass emails between the outbound and inbound mocks
            var unreadSentMails = new List<MailMessage>();
            var mockedImapClient = new Mock<IImapClient>();

            //When we are asked to fetch emails, return whatever we have in 'unreadSentEmails', then clear that list
            mockedImapClient.Setup(m => m.GetMessages(It.IsAny<IEnumerable<uint>>(),It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    var returnMails = new List<MailMessage>(unreadSentMails);
                    unreadSentMails.Clear();
                    return returnMails;
                });

            var mockedSendGridTransport = new Mock<ITransport>();
            //When we are asked to send an email, store it in unreadSentEmails
            mockedSendGridTransport.Setup(m => m.DeliverAsync(It.IsAny<ISendGrid>())).Callback<ISendGrid>(sgm => 
                unreadSentMails.Add(sgm.CreateMimeMessage())
                );

            ObjectFactory.Configure(o => o.For<IImapClient>().Use(mockedImapClient.Object));
            ObjectFactory.Configure(o => o.For<ITransport>().Use(mockedSendGridTransport.Object));

            //Create an email to be sent by the outbound email daemon
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Email email = ObjectFactory.GetInstance<Email>();
                var curEmail = email.GenerateBasicMessage(uow, uniqueCustomerEmailSubject, "Test bodyTest bodyTest bodyTest body", "me@gmail.com", "them@gmail.com");
                uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
                uow.SaveChanges();
            }

            //Run outbound daemon to make sure we get 
            var outboundEmailDaemon = new OutboundEmail();
            DaemonTests.RunDaemonOnce(outboundEmailDaemon);

            //Now the booking request should be created
            var inboundEmailDaemon = new InboundEmail();
            DaemonTests.RunDaemonOnce(inboundEmailDaemon);

            //Now, find the booking request
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetQuery().FirstOrDefault(br => br.Subject == uniqueCustomerEmailSubject);
                Assert.NotNull(bookingRequestDO, "Booking request was not created.");

                //Create an event
                var e = ObjectFactory.GetInstance<Event>();
                var eventDO = e.Create(uow, bookingRequestDO.Id, DateTime.Now.ToString(), DateTime.Now.AddHours(1).ToString());
                uow.SaveChanges();
                
                //Dispatch invites for the event
                e.GenerateInvitations(uow, eventDO, eventDO.Attendees);
                uow.SaveChanges();

                //Run our outbound email daemon so we can check if emails are created
                DaemonTests.RunDaemonOnce(outboundEmailDaemon);

                //Check each attendee recieves an invitation email
                foreach (var attendeeDO in eventDO.Attendees)
                {
                    Assert.True(unreadSentMails.Any(m => m.Subject.StartsWith("Invitation from me") && m.To.First().Address == attendeeDO.EmailAddress.Address), "Invitation not found for " + attendeeDO.Name);
                }
            }
        }

        [Test]
        [Category("IntegrationTests")]
        public void ITest_CanAddUserAndSendCredentialsEmail()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Creating a new user
                UserVM newUser = new UserVM();
                newUser.SendMail = true;
                newUser.NewPassword = "testpassword";
                newUser.FirstName = "FirstName";
                newUser.LastName = "LastName";
                newUser.EmailAddress = "test.user@kwasant.net";
                newUser.UserName = "testuser";

                //Calling controller method to Add user
                new UserController().ProcessAddUser(newUser);
                uow.SaveChanges();

                //Getting the envelop in queue
                EnvelopeDO queuedEnvelop = uow.EnvelopeRepository.GetAll().Where(e => e.Email.EmailStatus == EmailState.Queued).FirstOrDefault();
                string customerEmailSuject = "Kwasant Credentials";

                //This stores the emails which we have sent, but not yet read. It lets us pass emails between the outbound and inbound mocks
                var unreadSentMails = new List<MailMessage>();
                var mockedImapClient = new Mock<IImapClient>();

                //When we are asked to fetch emails, return whatever we have in 'unreadSentEmails', then clear that list
                mockedImapClient.Setup(m => m.GetMessages(It.IsAny<IEnumerable<uint>>(), It.IsAny<bool>(), It.IsAny<string>()))
                    .Returns(() =>
                    {
                        var returnMails = new List<MailMessage>(unreadSentMails);
                        unreadSentMails.Clear();
                        return returnMails;
                    });

                var mockedSendGridTransport = new Mock<ITransport>();
                //When we are asked to send an email, store it in unreadSentEmails
                mockedSendGridTransport.Setup(m => m.DeliverAsync(It.IsAny<ISendGrid>())).Callback<ISendGrid>(sgm => unreadSentMails.Add(sgm.CreateMimeMessage()));

                ObjectFactory.Configure(o => o.For<IImapClient>().Use(mockedImapClient.Object));
                ObjectFactory.Configure(o => o.For<ITransport>().Use(mockedSendGridTransport.Object));

                //Run outbound daemon to make sure we get 
                var outboundEmailDaemon = new OutboundEmail();
                DaemonTests.RunDaemonOnce(outboundEmailDaemon);

                var clientMock = new Mock<IImapClient>();

                var mailMessage = new MailMessage();
                mailMessage.Body = queuedEnvelop.Email.HTMLText;
                mailMessage.Subject = queuedEnvelop.Email.Subject;
                mailMessage.From = new MailAddress(queuedEnvelop.Email.From.Address);
                mailMessage.To.Add(new MailAddress(queuedEnvelop.Email.To.First().Address));

                clientMock.Setup(c => c.GetMessages(It.IsAny<IEnumerable<uint>>(), true, null))
                    .Returns(new List<MailMessage> { mailMessage });

                var imapClient = clientMock.Object;

                ObjectFactory.Configure(a => a.For<IImapClient>().Use(imapClient));

                //Now the booking request should be created, if created that means our new customer is created and get the mail with credentials.
                var inboundEmailDaemon = new InboundEmail();
                DaemonTests.RunDaemonOnce(inboundEmailDaemon);

                //Now, find the booking request
                Assert.AreEqual(1, uow.BookingRequestRepository.GetQuery().Where(br => br.Subject == customerEmailSuject).Count());
            }
        }

    }
}
