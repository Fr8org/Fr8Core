using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.UI;
using Data.Entities;
using Newtonsoft.Json.Linq;
using SendGrid;

namespace Core.Managers.APIManagers.Packagers.SendGrid
{
    public class SendGridPackager : IEmailPackager
    {
        private readonly ITransport _transport;

        public SendGridPackager(ITransport transport)
        {
            if (transport == null)
                throw new ArgumentNullException("transport");
            _transport = transport;
        }

        public delegate void EmailSuccessArgs(int emailID);
        public static event EmailSuccessArgs EmailSent;

        private static void OnEmailSent(int emailid)
        {
            EmailSuccessArgs handler = EmailSent;
            if (handler != null) handler(emailid);
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

        //Note that at the moment, we actually are submitting through SendGrid, not Gmail.
        public async void Send(EnvelopeDO envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException("envelope");
            if (!string.Equals(envelope.Handler, EnvelopeDO.SendGridHander, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(@"This envelope should not be handled with Gmail.", "envelope");
            if (envelope.Email == null)
                throw new ArgumentException(@"This envelope has no Email.", "envelope");
            if (envelope.Email.Recipients.Count == 0)
                throw new ArgumentException(@"This envelope has no recipients.", "envelope");
            
            var email = envelope.Email;
            if (email == null)
                throw new ArgumentException(@"Envelope email is null", "envelope");

            try
            {
                var fromName = !String.IsNullOrWhiteSpace(email.FromName) ? email.FromName : email.From.Name;

                var mailMessage = new SendGridMessage { From = new MailAddress(email.From.Address, fromName) };

                if (!String.IsNullOrWhiteSpace(email.ReplyToAddress))
                {
                    mailMessage.ReplyTo = new[] { new MailAddress(email.ReplyToAddress, email.ReplyToName) };
                }

                mailMessage.To = email.To.Select(toEmail => new MailAddress(toEmail.Address, toEmail.NameOrAddress())).ToArray();
                mailMessage.Bcc = email.BCC.Select(bcc => new MailAddress(bcc.Address, bcc.NameOrAddress())).ToArray();
                mailMessage.Cc = email.CC.Select(cc => new MailAddress(cc.Address, cc.NameOrAddress())).ToArray();

                mailMessage.Subject = email.Subject;

                if ((email.PlainText == null || email.HTMLText == null) && string.IsNullOrEmpty(envelope.TemplateName))
                {
                    throw new ArgumentException("Trying to send an email that doesn't have both an HTML and plain text body");
                }

                if (email.PlainText == null || email.HTMLText == null)
                {
                    mailMessage.Html = "<html></html>";
                    mailMessage.Text = "";
                }
                else
                {
                    mailMessage.Html = email.HTMLText;
                    mailMessage.Text = email.PlainText;
                }

                var headers = new Dictionary<String, String>();
                if (!String.IsNullOrEmpty(email.MessageID))
                    headers.Add("Message-ID", email.MessageID);
                if (!String.IsNullOrEmpty(email.References))
                    headers.Add("References", email.References);

                if (headers.Any())
                    mailMessage.AddHeaders(headers);

                foreach (var attachment in email.Attachments)
                {
                    mailMessage.AddAttachment(attachment.GetData(), attachment.OriginalName);
                }

                if (!string.IsNullOrEmpty(envelope.TemplateName))
                {
                    mailMessage.EnableTemplateEngine(envelope.TemplateName);//Now TemplateName will be TemplateId on Sendgrid.
                    if (envelope.MergeData != null)
                    {
                        //Now, we need to do some magic.
                        //Basically - we need the length of each substitution to match the length of recipients
                        //In our case, most of the time, all the substitutions are the same, except for token-related fields
                        //To make it easier to use, we attempt to pad out the substition arrays if they lengths don't match
                        //We only do that if we're given a string value. In any other case, we allow sengrid to fail.
                        var subs = new Dictionary<String, List<String>>();
                        foreach (var pair in envelope.MergeData)
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
                                for (var i = 0; i < email.Recipients.Count(); i++) //Pad out the substitution
                                    listVal.Add(pair.Value == null ? String.Empty : pair.Value.ToString());
                            }
                            subs.Add(pair.Key, listVal);
                            
                        }
                        foreach(var sub in subs)
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
