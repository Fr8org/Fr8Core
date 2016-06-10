using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Fr8.Infrastructure.Utilities;
using Newtonsoft.Json.Linq;
using SendGrid;
using StructureMap;
using terminalUtilities.Helpers;
using terminalUtilities.Interfaces;
using terminalUtilities.Models;

namespace terminalUtilities.SendGrid
{
    public class SendGridPackager : IEmailPackager
    {
        private readonly ITransport _transport;
        public SendGridPackager(IConfigRepository configRepository)
        {
            _transport = TransportFactory.CreateWeb(configRepository);
        }
        public delegate void EmailSuccessArgs(int emailID);
        public static event EmailSuccessArgs EmailSent;
        private static void OnEmailSent(int emailid)
        {
            EmailSuccessArgs handler = EmailSent;
            handler?.Invoke(emailid);
        }
        public delegate void EmailRejectedArgs(string rejectReason, int emailID);
        public static event EmailRejectedArgs EmailRejected;
        private static void OnEmailRejected(string rejectReason, int emailid)
        {
            EmailRejectedArgs handler = EmailRejected;
            if (handler != null) handler(rejectReason, emailid);
        }
        public delegate void EmailCriticalErrorArgs(int errorCode, string name, string message, int emailID);
        public static event EmailCriticalErrorArgs EmailCriticalError;
        private static void OnEmailCriticalError(int errorCode, string name, string message, int emailID)
        {
            EmailCriticalErrorArgs handler = EmailCriticalError;
            if (handler != null) handler(errorCode, name, message, emailID);
        }
        public async Task Send(TerminalMailerDO mailer)
        {
            if (mailer == null)
                throw new ArgumentNullException("mailer");
            if (mailer.Email == null)
                throw new ArgumentException(@"This envelope has no Email.", nameof(mailer));
            if (!mailer.Email.GetToRecipients().Any())
                throw new ArgumentException(@"This envelope has no recipients.", nameof(mailer));
            var email = mailer.Email;
            try
            {
                var fromName = email.From.Name;
                var mailMessage = new SendGridMessage { From = new MailAddress(email.From.Address, fromName) };
                if (!String.IsNullOrWhiteSpace(email.From.Address))
                {
                    mailMessage.ReplyTo = new[] { new MailAddress(email.From.Address, fromName) };
                }
                mailMessage.To = email.GetToRecipients().Select(toEmail => new MailAddress(toEmail.Address, toEmail.Name ?? toEmail.Address)).ToArray();
                mailMessage.Subject = email.Subject;
                if ((email.HTMLText == null) && string.IsNullOrEmpty(mailer.TemplateName))
                {
                    throw new ArgumentException(
                        "Trying to send an email that doesn't have both an HTML and plain text body");
                }
                if (email.HTMLText == null)
                {
                    mailMessage.Html = "<html></html>";
                    mailMessage.Text = "";
                }
                else
                {
                    mailMessage.Html = email.HTMLText;
                }
                var headers = new Dictionary<String, String>();
                if (headers.Any())
                    mailMessage.AddHeaders(headers);
                if (mailer.Footer.Any())
                    mailMessage.EnableFooter(null, mailer.Footer);
                if (!string.IsNullOrEmpty(mailer.TemplateName))
                {
                    mailMessage.EnableTemplateEngine(mailer.TemplateName);
                    //Now TemplateName will be TemplateId on Sendgrid.
                    if (mailer.MergeData != null)
                    {
                        //Now, we need to do some magic.
                        //Basically - we need the length of each substitution to match the length of recipients
                        //In our case, most of the time, all the substitutions are the same, except for token-related fields
                        //To make it easier to use, we attempt to pad out the substition arrays if they lengths don't match
                        //We only do that if we're given a string value. In any other case, we allow sengrid to fail.
                        var subs = new Dictionary<String, List<String>>();
                        foreach (var pair in mailer.MergeData)
                        {
                            var arrayType = pair.Value as JArray;
                            List<String> listVal;
                            if (arrayType != null)
                            {
                                listVal = arrayType.Select(a => a.ToString()).ToList();
                            }
                            else
                            {
                                listVal = new List<string>();
                                for (var i = 0; i < email.GetToRecipients().Count(); i++) //Pad out the substitution
                                    listVal.Add(pair.Value == null ? String.Empty : pair.Value.ToString());
                            }
                            subs.Add(pair.Key, listVal);
                        }
                        foreach (var sub in subs)
                            mailMessage.AddSubstitution(sub.Key, sub.Value);
                    }
                }
                try
                {
                    await _transport.DeliverAsync(mailMessage);
                    OnEmailSent(email.Id);
                }
                catch (Exception ex)
                {
                    OnEmailRejected(ex.Message, email.Id);
                }
            }
            catch (Exception ex)
            {
                OnEmailCriticalError(-1, "Unhandled exception.", ex.Message, email.Id);
            }
        }
    }
}