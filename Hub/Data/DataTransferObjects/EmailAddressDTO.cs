using System;
using System.Collections.Generic;

namespace Fr8Data.DataTransferObjects
{
    public class EmailAddressDTO
    {
        private string emailAddress;

        public EmailAddressDTO()
        {
        }
        public EmailAddressDTO(string emailAddress)
        {
            this.emailAddress = emailAddress;
        }

        public int Id { get; set; }
        public String Name { get; set; }
        public String Address { get; set; }
        public virtual List<RecipientDTO> Recipients { get; set; }
        public virtual List<EmailDTO> SentEmails { get; set; }
    }
}
