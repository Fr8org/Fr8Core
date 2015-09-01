using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Wrappers;
using Utilities;

namespace Core.Services
{
    public class EmailAddress : IEmailAddress
    {
        private readonly IConfigRepository _configRepository;

        public EmailAddress(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
        }

        public EmailAddressDO ConvertFromMailAddress(IUnitOfWork uow, MailAddress address)
        {
            return uow.EmailAddressRepository.GetOrCreateEmailAddress(address.Address, address.DisplayName);
        }

        public List<ParsedEmailAddress> ExtractParsedFromString(params String[] textsToSearch)
        {
            var returnList = new List<ParsedEmailAddress>();
            foreach (var textToSearch in textsToSearch)
            {
                returnList.AddRange(ExtractFromString(textToSearch).Select(pea => pea));
            }
            return returnList.GroupBy(pea => pea.Email).Select(g => g.First()).ToList(); //Distinct
        }
        
        public List<ParsedEmailAddress> ExtractFromString(String textToSearch)
        {
            var ru = new RegexUtilities();
            return ru.ExtractFromString(textToSearch);
        }

        public List<EmailAddressDO> GetEmailAddresses(IUnitOfWork uow, params string[] textToSearch)
        {
            var emailAddresses = ExtractParsedFromString(textToSearch);

            var addressList =
                emailAddresses.Select(
                    parsedEmailAddress =>
                        uow.EmailAddressRepository.GetOrCreateEmailAddress(parsedEmailAddress.Email,
                            parsedEmailAddress.Name));

            return addressList.ToList();
        }

        public EmailAddressDO ConvertFromString(string emailString, IUnitOfWork uow)
        {
            String email = string.Empty;
            String name = string.Empty;
            emailString = emailString.Replace("\"", string.Empty);
            if (emailString.Contains("<"))
            {
                string[] parts = emailString.Split('<');
                name = parts[0];
                email = parts[1];
                email = email.Replace(">", string.Empty);
            }
            else
                email = emailString;

            EmailAddressDO convertAddresFromString = uow.EmailAddressRepository.GetOrCreateEmailAddress(email, name);
            return convertAddresFromString;
        }


        public EmailAddressDO GetFromEmailAddress(IUnitOfWork uow, EmailAddressDO curRecipientAddress, DockyardAccountDO originator)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (curRecipientAddress == null)
                throw new ArgumentNullException("curRecipientAddress");
            var user = new DockyardAccount();
            var curRecipient = uow.UserRepository.GetOrCreateUser(curRecipientAddress);
            if (curRecipient != null)
            {
                var communicationMode = user.GetMode(curRecipient);
                switch (communicationMode)
                {
                    case CommunicationMode.Direct:
                        return uow.EmailAddressRepository.GetOrCreateEmailAddress(
                            _configRepository.Get("EmailFromAddress_DirectMode"),
                            _configRepository.Get("EmailFromName_DirectMode"));
                    case CommunicationMode.Delegate:
                        return uow.EmailAddressRepository.GetOrCreateEmailAddress(
                            _configRepository.Get("EmailFromAddress_DelegateMode"),
                            String.Format(_configRepository.Get("EmailFromName_DelegateMode"),
                                DockyardAccount.GetDisplayName(originator)));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return uow.EmailAddressRepository.GetOrCreateEmailAddress(
                    _configRepository.Get("EmailFromAddress_DelegateMode"),
                    String.Format(_configRepository.Get("EmailFromName_DelegateMode"), DockyardAccount.GetDisplayName(originator)));
            }
        }
    }
}
