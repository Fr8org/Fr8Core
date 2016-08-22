using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;

namespace Hub.Services
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
            return RegexUtilities.ExtractEmailFromString(_configRepository, textToSearch);
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

    }
}
