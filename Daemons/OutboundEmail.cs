using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Daemons.EventExposers;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Hub.Managers.APIManagers.Packagers;
using Hub.Services;
using Utilities;
using Utilities.Logging;

namespace Daemons
{
    public class OutboundEmail : Daemon<OutboundEmail>
    {
        //private string logString;

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
                        //Logger.GetLogger().Error("Email id " + emailID + " received a callback saying it was sent from Gmail, but the email was not found in our database");
                        Logger.LogError("Email id " + emailID + " received a callback saying it was sent from Gmail, but the email was not found in our database");
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
		    while (ProcessNextEventNoWait())
		    {
		    }

            using (IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var configRepository = ObjectFactory.GetInstance<IConfigRepository>();

                //A Mailer is essentially an envelope: it's an email plus a bunch of handler and template information so we know where to dispatch the email
                EnvelopeRepository envelopeRepository = unitOfWork.EnvelopeRepository;
                var numSent = 0;


                //each queued message is processed
                foreach ( EnvelopeDO curMailerDO in envelopeRepository.FindList(e => e.Email.EmailStatus == EmailState.Queued))
                {
                    LogEvent("Sending an email with subject '" + curMailerDO.Email.Subject + "'");
                    using (var subUow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        var mailer = subUow.EnvelopeRepository.GetByKey(curMailerDO.Id);
                        try
                        {
                            // we have to query EnvelopeDO one more time to have it loaded in subUow
                            IEmailPackager packager = ObjectFactory.GetNamedInstance<IEmailPackager>(mailer.Handler);

                            if (configRepository.Get<bool>("ArchiveOutboundEmail"))
                            {
                                EmailAddressDO outboundemailaddress =
                                    subUow.EmailAddressRepository.GetOrCreateEmailAddress(
                                        configRepository.Get("ArchiveEmailAddress"), "Outbound Archive");
                                mailer.Email.AddEmailRecipient(EmailParticipantType.Bcc, outboundemailaddress);
                            }

                            //Removing email address which are not test account in debug mode
                            if (Server.IsDevMode)
                            {
                                var recipientsRemoved = RemoveRecipients(mailer.Email, subUow);
                                if (recipientsRemoved.Any())
                                {
                                    var message =
                                        String.Format(
                                            "The following recipients were removed because they are not test accounts: {0}",
                                            String.Join(", ", recipientsRemoved));
                                    //Logger.GetLogger().Info(message);
                                    Logger.LogInfo(message);
                                    LogEvent(message);
                                }
                            }

                            if (String.IsNullOrEmpty(mailer.Email.ReplyToAddress))
                                mailer.Email.ReplyToAddress = configRepository.Get("replyToEmail", String.Empty);
                            if (String.IsNullOrEmpty(mailer.Email.ReplyToName))
                                mailer.Email.ReplyToName = configRepository.Get("replyToName", String.Empty);

                            packager.Send(mailer);
                            numSent++;

                            var email = mailer.Email;
                            email.EmailStatus = EmailState.Dispatched;
                            subUow.SaveChanges();

                            LogSuccess("Sent.");

                            foreach (var recipient in email.To)
                            {
                                var curUser = subUow.UserRepository.GetQuery()
                                    .FirstOrDefault(u => u.EmailAddressID == recipient.Id);
                                if (curUser != null)
                                {
                                    EventManager.EmailSent(email.Id, curUser.Id);
                                }
                            }
                        }
                        catch (StructureMapConfigurationException ex)
                        {
                            //Logger.GetLogger().ErrorFormat("Unknown email packager: {0}", curMailerDO.Handler);
                            Logger.LogError($"Unknown email packager: {curMailerDO.Handler}");

                            try
                            {
                                var email = mailer.Email;
                                email.EmailStatus = EmailState.Invalid;
                                subUow.SaveChanges();
                            }
                            catch (Exception)
                            {
                            }

                            throw new UnknownEmailPackagerException(
                                string.Format("Unknown email packager: {0}", curMailerDO.Handler), ex);
                        }
                    }
                }
            }
        }

        private List<String> RemoveRecipients(EmailDO emailDO, IUnitOfWork uow)
        {
            var recipientsRemoved = new List<String>();
            var recipientList = emailDO.Recipients.ToList();
            
            foreach (RecipientDO recipient in recipientList)
            {
                Fr8AccountDO dockyardAccount = uow.UserRepository.FindOne(e => e.EmailAddress.Address == recipient.EmailAddress.Address);

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
                //Logger.GetLogger().Error(logMessage);
                Logger.LogError(logMessage);
            }
            return emailDO;
        }
    }
}
