using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Linq;
using SendGrid;

namespace HealthMonitor
{
    public class TestReportNotifier
    {
        public string GetFromEmailAddress()
        {
            return ConfigurationManager.AppSettings["OutboundFromAddress"];
        }

        public string GetFromName()
        {
            return GetFromEmailAddress();
        }

        public string GetUserName()
        {
            return ConfigurationManager.AppSettings["OutboundUserName"];
        }

        public string GetPassword()
        {
            return ConfigurationManager.AppSettings["OutboundUserPassword"];
        }

        public string[] GetToEmails()
        {
            var toEmailsString = ConfigurationManager.AppSettings["NotificationEmails"];
            var toEmails = toEmailsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            return toEmails;
        }

        public string CreateSubject()
        {
            return string.Format("Test report {0:R}", DateTime.Now);
        }

        public async void Notify(string htmlReport)
        {
            var mailMessage = new SendGridMessage
            {
                From = new MailAddress(GetFromEmailAddress(), GetFromName()),
                ReplyTo = new[] { new MailAddress(GetFromEmailAddress(), GetFromName()) },
                To = GetToEmails().Select(x => new MailAddress(x)).ToArray()
            };

            mailMessage.Subject = CreateSubject();
            mailMessage.Html = htmlReport;

            var credentials = new NetworkCredential
            {
                UserName = GetUserName(),
                Password = GetPassword()
            };

            var web = new Web(credentials);
            await web.DeliverAsync(mailMessage);
        }
    }
}
