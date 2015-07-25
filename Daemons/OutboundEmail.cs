using System;
using System.Collections.Generic;
using System.Linq;
using Daemons.EventExposers;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Core.Managers.APIManagers.Packagers;
using StructureMap;
using Utilities;
using Utilities.Logging;
using Data.Infrastructure;
using Core.Services;

namespace Daemons
{
    public class OutboundEmail : Daemon<OutboundEmail>
    {
        private string logString;

        public OutboundEmail()
        {
            RegisterEvent<int>(SendGridPackagerEventHandler.EmailSent, emailID =>
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    EmailRepository emailRepository = uow.EmailRepository;
                    var emailToUpdate = emailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
                    if (emailToUpdate == null)
                    {
                        Logger.GetLogger().Error("Email id " + emailID + " received a callback saying it was sent from Gmail, but the email was not found in our database");
                        return;
                    }

                    emailToUpdate.EmailStatus = EmailState.Sent;
                    uow.SaveChanges();
                }
            });

            RegisterEvent<string, int>(SendGridPackagerEventHandler.EmailRejected, (reason, emailID) =>
            {
                LogFail(null, reason);
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    string logMessage = String.Format("Email was rejected with id '{0}'. Reason: {1}", emailID, reason);
                    var emailToUpdate = ProcessMandrillError(uow, logMessage, emailID);
                    emailToUpdate.EmailStatus = EmailState.SendRejected;
                    uow.SaveChanges();
                }
            });

            RegisterEvent<int, string, string, int>(SendGridPackagerEventHandler.EmailCriticalError,
                (errorCode, name, message, emailID) =>
                {
                    LogFail(null, message);
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        string logMessage = String.Format("Email failed. Error code: {0}. Name: {1}. Message: {2}. EmailID: {3}", errorCode, name, message, emailID);
                        var emailToUpdate = ProcessMandrillError(uow, logMessage, emailID);

                        emailToUpdate.EmailStatus = EmailState.SendCriticalError;
                        
                        uow.SaveChanges();
                    }

                    EventManager.Error_EmailSendFailure(emailID, message);
                });

            AddTest("OutboundEmailDaemon_Test", "Test");
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
		//while (ProcessNextEventNoWait())
		//{
		//}

		//using (IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
		//{
		//    var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
		//    EnvelopeRepository envelopeRepository = unitOfWork.EnvelopeRepository;
		//    var numSent = 0;
		//    foreach (EnvelopeDO curEnvelopeDO in envelopeRepository.FindList(e => e.Email.EmailStatus == EmailState.Queued))
		//    {
		//	  LogEvent("Sending an email with subject '" + curEnvelopeDO.Email.Subject + "'");
		//	  using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
		//	  {
		//		var envelope = subUow.EnvelopeRepository.GetByKey(curEnvelopeDO.Id);
		//		try
		//		{
		//		    // we have to query EnvelopeDO one more time to have it loaded in subUow
		//		    IEmailPackager packager = ObjectFactory.GetNamedInstance<IEmailPackager>(envelope.Handler);
		//		    if (configRepository.Get<bool>("ArchiveOutboundEmail"))
		//		    {
		//			  EmailAddressDO outboundemailaddress = subUow.EmailAddressRepository.GetOrCreateEmailAddress(configRepository.Get("ArchiveEmailAddress"), "Outbound Archive");
		//			  envelope.Email.AddEmailRecipient(EmailParticipantType.Bcc, outboundemailaddress);
		//		    }

		//		    //Removing email address which are not test account in debug mode
		//		    if (Server.IsDevMode)
		//		    {
		//			  var recipientsRemoved = RemoveRecipients(envelope.Email, subUow);
		//			  if (recipientsRemoved.Any())
		//			  {
		//				var message = String.Format("The following recipients were removed because they are not test accounts: {0}", String.Join(", ", recipientsRemoved));
		//				Logger.GetLogger().Info(message);
		//				LogEvent(message);
		//			  }
		//		    }

		//		    if (String.IsNullOrEmpty(envelope.Email.ReplyToAddress))
		//			  envelope.Email.ReplyToAddress = configRepository.Get("replyToEmail", String.Empty);
		//		    if (String.IsNullOrEmpty(envelope.Email.ReplyToName))
		//			  envelope.Email.ReplyToName = configRepository.Get("replyToName", String.Empty);
                            
		//		    packager.Send(envelope);
		//		    numSent++;

		//		    var email = envelope.Email;
		//		    email.EmailStatus = EmailState.Dispatched;
		//		    subUow.SaveChanges();

		//		    LogSuccess("Sent.");

		//		    foreach (var recipient in email.To)
		//		    {
		//			  var curUser = subUow.UserRepository.GetQuery()
		//				.FirstOrDefault(u => u.EmailAddressID == recipient.Id);
		//			  if (curUser != null)
		//			  {
		//				AlertManager.EmailSent(email.Id, curUser.Id);
		//			  }
		//		    }
		//		}
		//		catch (StructureMapConfigurationException ex)
		//		{
		//		    Logger.GetLogger().ErrorFormat("Unknown email packager: {0}", curEnvelopeDO.Handler);

		//		    try
		//		    {
		//			  var email = envelope.Email;
		//			  email.EmailStatus = EmailState.Invalid;
		//			  subUow.SaveChanges();
		//		    } catch (Exception) {}

		//		    throw new UnknownEmailPackagerException(string.Format("Unknown email packager: {0}", curEnvelopeDO.Handler), ex);
		//		}
		//	  }
		//    }
		//}
        }

        private List<String> RemoveRecipients(EmailDO emailDO, IUnitOfWork uow)
        {
            var recipientsRemoved = new List<String>();
            var recipientList = emailDO.Recipients.ToList();
            
            foreach (RecipientDO recipient in recipientList)
            {
                DockyardAccountDO dockyardAccount = uow.UserRepository.FindOne(e => e.EmailAddress.Address == recipient.EmailAddress.Address);

                if (dockyardAccount != null && !uow.AspNetUserRolesRepository.UserHasRole(Roles.Admin, dockyardAccount.Id) && !dockyardAccount.TestAccount)
                {
                    recipientsRemoved.Add(recipient.EmailAddress.Address);
                    uow.RecipientRepository.Remove(recipient);
                }
            }
            return recipientsRemoved;
        }


        public EmailDO ProcessMandrillError(IUnitOfWork uow, string logMessage, int emailID)
        {
            var emailDO = uow.EmailRepository.GetQuery().FirstOrDefault(e => e.Id == emailID);
            if (emailDO == null)
            {
                Logger.GetLogger().Error(logMessage);
            }
            return emailDO;
        }
    }
}
