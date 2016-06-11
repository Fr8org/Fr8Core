using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Fr8.Infrastructure.Utilities;
using Hub.Managers.APIManagers.Packagers;


namespace Hub.Services
{
    public class Email
    {
        private readonly IConfigRepository _configRepository;

        public const string DateStandardFormat = @"yyyy-MM-ddTHH\:mm\:ss.fffffff"; //This allows javascript to parse the date properly
        //private EventValidator _curEventValidator;


        public Email(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        #region Method

        public void Send(IUnitOfWork uow, EmailDO emailDO)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
            uow.SaveChanges();
        }

        public async Task SendAsync(IUnitOfWork uow, string subject, string message, string fromAddress, string toAddress)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            var curEmail = GenerateBasicMessage(uow, subject, message, fromAddress, toAddress);

            uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
            uow.SaveChanges();

            await ObjectFactory.GetInstance<IEmailPackager>().Send(new EnvelopeDO { Email = curEmail });
        }
        

        public void Send(IUnitOfWork uow, string subject, string message, string fromAddress, string toAddress)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            var curEmail = GenerateBasicMessage(uow, subject, message, fromAddress, toAddress);
            Send(uow, curEmail);
        }

        public void SendUserSettingsNotification(IUnitOfWork uow, Fr8AccountDO submittedDockyardAccountData) 
        {
            EmailDO curEmail = new EmailDO();
            curEmail.From = submittedDockyardAccountData.EmailAddress;
            curEmail.AddEmailRecipient(EmailParticipantType.To, submittedDockyardAccountData.EmailAddress);
            curEmail.Subject = "DockYardAccount Settings Notification";
            //new Email(uow).SendTemplate(uow, "User_Settings_Notification", curEmail, null);
            //uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmail, "User_Settings_Notification", null);
        }

        #endregion

      
        public static EmailDO ConvertMailMessageToEmail(IEmailRepository emailRepository, MailMessage mailMessage)
        {
            return ConvertMailMessageToEmail<EmailDO>(emailRepository, mailMessage);            
        }

        public static TEmailType ConvertMailMessageToEmail<TEmailType>(IGenericRepository<TEmailType> emailRepository, MailMessage mailMessage)
            where TEmailType : EmailDO, new()
        {
            String body = String.Empty;
            String plainBody = mailMessage.Body;
            if (!mailMessage.IsBodyHtml)
            {
                foreach (var av in mailMessage.AlternateViews)
                {
                    av.ContentStream.Position = 0;
                    if (av.ContentType.MediaType == "text/html")
                    {
                        body = new StreamReader(av.ContentStream).ReadToEnd();
                        break;
                    }

                    if (av.ContentType.MediaType == "text/plain")
                    {
                        plainBody = new StreamReader(av.ContentStream).ReadToEnd();
                    }
                }
            }
            if (String.IsNullOrEmpty(body))
                body = mailMessage.Body;

            String strDateReceived = String.Empty;
            strDateReceived = mailMessage.Headers["Date"];

            DateTimeOffset dateReceived;
            if (!DateTimeOffset.TryParse(strDateReceived, out dateReceived))
                dateReceived = DateTimeOffset.UtcNow;

            String strDateCreated = String.Empty;
            strDateCreated = mailMessage.Headers["Date"];

            DateTimeOffset dateCreated;
            if (!DateTimeOffset.TryParse(strDateCreated, out dateCreated))
                dateCreated = default(DateTimeOffset);

            TEmailType emailDO = new TEmailType
            {                
                Subject = mailMessage.Subject,
                HTMLText = body,
                PlainText = plainBody,
                DateReceived = dateReceived,
                CreateDate = dateCreated,
                Attachments = mailMessage.Attachments.Select(CreateNewAttachment).Union(mailMessage.AlternateViews.Select(CreateNewAttachment)).Where(a => a != null).ToList()
            };


            emailDO.MessageID = mailMessage.Headers["Message-ID"];
            emailDO.References = mailMessage.Headers["References"];

            var uow = emailRepository.UnitOfWork;

            var fromAddress = GenerateEmailAddress(uow, mailMessage.From);
            emailDO.From = fromAddress;
            
            foreach (var addr in mailMessage.To.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.To, addr);    
            }
            foreach (var addr in mailMessage.Bcc.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.Bcc, addr);
            }
            foreach (var addr in mailMessage.CC.Select(a => GenerateEmailAddress(uow, a)))
            {
                emailDO.AddEmailRecipient(EmailParticipantType.Cc, addr);
            }

            emailDO.Attachments.ForEach(a => a.Email = emailDO);
            
            emailDO.EmailStatus = EmailState.Unstarted; //we'll use this new state so that every email has a valid status.
            emailRepository.Add(emailDO);
            
            return emailDO;
        }

        public static EmailAddressDO GenerateEmailAddress(IUnitOfWork uow, MailAddress address)
        {
            return uow.EmailAddressRepository.GetOrCreateEmailAddress(address.Address, address.DisplayName);
        }

        public static AttachmentDO CreateNewAttachment(Attachment attachment)
        {
            AttachmentDO att = new AttachmentDO
            {
                OriginalName = attachment.Name,
                Type = attachment.ContentType.MediaType,
            };
            
            att.SetData(attachment.ContentStream);
            return att;
        }

        public static AttachmentDO CreateNewAttachment(AlternateView av)
        {
            if (av.ContentType.MediaType == "text/html")
                return null;

            AttachmentDO att = new AttachmentDO
            {
                OriginalName = String.IsNullOrEmpty(av.ContentType.Name) ? "unnamed" : av.ContentType.Name,
                Type = av.ContentType.MediaType,
                ContentID = av.ContentId
            };

            att.SetData(av.ContentStream);
            return att;
        }
        
        public EmailDO GenerateBasicMessage(IUnitOfWork uow, string subject, string message, string fromAddress ,string toRecipient)
        {
            RegexUtilities.ValidateEmailAddress(_configRepository, toRecipient);
            EmailDO curEmail = new EmailDO
            {
                Subject = subject,
                PlainText = message,
                HTMLText = message
            };
            curEmail = AddFromAddress(uow, curEmail,fromAddress);
            curEmail = AddSingleRecipient(uow, curEmail, toRecipient);
            return curEmail;
        }

        public void SendAlertEmail(string subject, string message = null)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");

                EmailDO curEmail = new EmailDO();
                curEmail = GenerateBasicMessage(uow, subject, message ?? subject, fromAddress, "ops@kwasant.com");
                uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
                uow.SaveChanges();
            }
        }

        public EmailDO AddSingleRecipient(IUnitOfWork uow, EmailDO curEmail, string toRecipient)
        {
            curEmail.Recipients = new List<RecipientDO>()
                                         {
                                              new RecipientDO()
                                                 {
                                                   EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(toRecipient),
                                                   EmailParticipantType = EmailParticipantType.To
                                                 }
                                         };
            return curEmail;
        }


        public EmailDO AddFromAddress(IUnitOfWork uow, EmailDO curEmail, string fromAddress)
        {
            var from = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress);
            curEmail.From = from;
            curEmail.FromID = from.Id;
            return curEmail;
        }


		/*
		 * Currently not in use, left for future reference
		 * 
        internal static void FixInlineImages(EmailDO currEmailDO)
        {
            //Fix the HTML text
            var attachmentSubstitutions =
                currEmailDO.Attachments.Where(a => !String.IsNullOrEmpty(a.ContentID))
                    .ToDictionary(a => a.ContentID, a => a.Id);

            string fileViewURLStr = Server.ServerUrl + "Api/GetAttachment.ashx?AttachmentID={0}";

            //The following fixes inline images
            if (attachmentSubstitutions.Any())
            {
                var curBody = currEmailDO.HTMLText;
                foreach (var keyToReplace in attachmentSubstitutions.Keys)
                {
                    var keyStr = String.Format("cid:{0}", keyToReplace);
                    curBody = curBody.Replace(keyStr,
                        String.Format(fileViewURLStr, attachmentSubstitutions[keyToReplace]));
                }
                currEmailDO.HTMLText = curBody;
            }
        }
		*/


		/*
		 * Currently not in use, left for future reference
		 * 
        public void SendLoginCredentials(IUnitOfWork uow, string toRecipient, string newPassword) 
        {
            string credentials = "<br/> Email : " + toRecipient + "<br/> Password : " + newPassword;
            string fromAddress = ObjectFactory.GetInstance<IConfigRepository>().Get("EmailAddress_GeneralInfo");
            EmailDO emailDO = GenerateBasicMessage(uow, "Kwasant Credentials", null, fromAddress, toRecipient);
		    uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, ObjectFactory.GetInstance<IConfigRepository>().Get("user_credentials"),
		    	  new Dictionary<string, object>
		    	  {
		    		{"credentials_string", credentials}
		    	  });
            uow.SaveChanges();
        }
		*/


		/*
		 * Currently not in use, left for future reference
		 * 
        public string FindEmailParentage(int emailId) 
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return Convert.ToString(uow.EmailRepository.GetByKey(emailId).Id);
            }
        }
		*/


		/*
		 * Currently not in use, left for future reference
		 * 
        public List<object> GetEmails(IUnitOfWork uow, DateRange dateRange, int start, int length, out int count)
        {
            var emailDO = uow.EmailRepository.GetAll()
                .Where(e => e.CreateDate > dateRange.StartTime && e.CreateDate < dateRange.EndTime);

            count = emailDO.Count();

            return emailDO.Skip(start).OrderByDescending(e => e.DateReceived).Take(length)
                .Select(e => (object)new
                {
                    Id = e.Id,
                    From = uow.EmailAddressRepository.GetByKey(e.FromID).Address, 
                    Subject = e.Subject,
                    Date = e.CreateDate.ToString(DateStandardFormat),
                    EmailStatus = FilterUtility.GetState(new EmailState().GetType(), (e.EmailStatus.HasValue ? e.EmailStatus.Value : 0)),
                    //EmailStatus = "",
                    ConversationId = e.Id
                })
				.ToList();
        }
		*/
    }
}
