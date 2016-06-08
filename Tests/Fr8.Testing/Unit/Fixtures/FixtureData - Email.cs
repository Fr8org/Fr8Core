using System.Net.Mail;
using Data.Entities;
using Data.States;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static EmailDO TestEmail1()
        {

            EmailDO curEmailDO = new EmailDO();
            curEmailDO.Id = 1;
            curEmailDO.From = TestEmailAddress1();
            curEmailDO.AddEmailRecipient(EmailParticipantType.To, TestEmailAddress2());
            curEmailDO.Subject = "Main Subject";
            curEmailDO.HTMLText = "This is the Body Text";
            return curEmailDO;

        }

        public static EmailDO TestEmail3()
        {

            EmailDO curEmailDO = new EmailDO();
            curEmailDO.Id = 3;
            curEmailDO.From = TestEmailAddress3();
            curEmailDO.AddEmailRecipient(EmailParticipantType.To, TestEmailAddress3());
            curEmailDO.Subject = "Main Subject";
            curEmailDO.HTMLText = "This Email is intended to be used with KwasantIntegration account ";
            return curEmailDO;

        }

        public static MailMessage TestMessage1()
        {
            string testSubject = "Conversation Test Subject";
            string testBody = "Conversation Test Body";
            var mailMessage1 = new MailMessage();
            mailMessage1.Body = testBody;
            mailMessage1.Subject = testSubject;
            mailMessage1.From = new MailAddress("a@gmail.com");
            mailMessage1.To.Add(new MailAddress("b@gmail.com"));
            mailMessage1.To.Add(new MailAddress("c@gmail.com"));
            return mailMessage1;
        }

        public static MailMessage TestMessage2()
        {
            string testSubject = "Conversation Test Subject";
            string testBody = "Conversation Test Body";
            var mailMessage2 = new MailMessage();
            mailMessage2.Body = testBody;
            mailMessage2.Subject = testSubject;
            mailMessage2.From = new MailAddress("b@gmail.com");
            mailMessage2.To.Add(new MailAddress("a@gmail.com"));
            mailMessage2.To.Add(new MailAddress("c@gmail.com"));
            return mailMessage2;
        }

    }
}

