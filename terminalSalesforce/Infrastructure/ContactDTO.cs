using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalSalesforce.Infrastructure
{
    public class ContactDTO
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MobilePhone { get; set; }

        public string Email { get; set; }
        public string AccountId { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string HomePhone { get; set; }
        public string OtherPhone { get; set; }
        public string Fax { get; set; }
        public string MailingStreet { get; set; }
        public string MailingCity { get; set; }
        public string MailingState { get; set; }
        public string MailingPostalCode { get; set; }
        public string MailingCountry { get; set; }
        public string OtherStreet { get; set; }
        public string OtherCity { get; set; }
        public string OtherState { get; set; }
        public string OtherPostalCode { get; set; }
        public string OtherCountry { get; set; }
    }
}