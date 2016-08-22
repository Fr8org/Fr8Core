using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SendGrid;
using StructureMap;
using Data.Infrastructure;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.Infrastructure.Utilities.Configuration;

namespace Hub.Managers.APIManagers.Packagers.SendGrid
{
    public class SendGridPackager : IEmailPackager
    {
        private readonly ITransport _transport;

        public SendGridPackager()
        {
            _transport = TransportFactory.CreateWeb(ObjectFactory.GetInstance<IConfigRepository>());
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

        public async Task Send(IMailerDO mailer)
        {
            if (mailer == null)
                throw new ArgumentNullException("mailer");
            if (mailer.Email == null)
                throw new ArgumentException(@"This envelope has no Email.", "mailer");
            if (mailer.Email.To.Count() == 0)
                throw new ArgumentException(@"This envelope has no recipients.", "mailer");

            var email = mailer.Email;

            try
            {
                var fromName = email.From.Name;

                var mailMessage = new SendGridMessage { From = new MailAddress(email.From.Address, fromName) };

                if (!String.IsNullOrWhiteSpace(email.From.Address))
                {
                    mailMessage.ReplyTo = new[] { new MailAddress(email.From.Address, fromName) };
                }

                mailMessage.To =
                    email.To.Select(toEmail => new MailAddress(toEmail.Address, toEmail.NameOrAddress())).ToArray();

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
                                for (var i = 0; i < email.To.Count(); i++) //Pad out the substitution
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
                    Logger.GetLogger().Error("Error occured while trying to send email. " +
                                    $"From = {email.From.Address}; " +
                                    $"Subject = {email.Subject}; " +
                                    $"Exception = {ex.Message}; ");
                    EventManager.Error_EmailSendFailure(email.Id, ex.Message);
                }
            }
            catch (Exception ex)
            {
                OnEmailCriticalError(-1, "Unhandled exception.", ex.Message, email.Id);
            }
        }
    }
}