using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Linq;
using SendGrid;
using HealthMonitor.Configuration;

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
            var healthMoniorCS = (HealthMonitorConfigurationSection)
                ConfigurationManager .GetSection("healthMonitor");

            if (healthMoniorCS == null || healthMoniorCS.Notifiers == null)
            {
                return null;
            }

            var notifiers = healthMoniorCS.Notifiers.Select(x => x.Email).ToArray();
            return notifiers;
        }

        public string CreateSubject(string appName)
        {
            return string.Format("{0}, Test report {1:R}", appName, DateTime.Now);
        }

        public void Notify(string appName, string htmlReport, string username, string password)
        {
            /**
            This code is for the future
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Trace.TraceWarning($"The SMTP username [{username}] or password [{password}] is empty. No email will be sent.");
                return;
            }
            */

            var toEmails = GetToEmails();
            if (toEmails == null)
            {
                return;
            }

            var mailMessage = new SendGridMessage
            {
                From = new MailAddress(GetFromEmailAddress(), GetFromName()),
                ReplyTo = new[] { new MailAddress(GetFromEmailAddress(), GetFromName()) },
                To = toEmails.Select(x => new MailAddress(x)).ToArray()
            };

            mailMessage.Subject = CreateSubject(appName);
            mailMessage.Html = htmlReport;

            var credentials = new NetworkCredential
            {
                UserName = GetUserName(),
                Password = GetPassword()
            };

            var web = new Web(credentials);
            web.Deliver(mailMessage);
        }
    }
}
