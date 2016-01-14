using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class EmailAddressDO : BaseObject, IEmailAddressDO
    {
        public EmailAddressDO()
        {
            Recipients = new List<RecipientDO>();
            SentEmails = new List<EmailDO>();
        }

        public EmailAddressDO(string emailAddress) : this()
        {
            Address = emailAddress;
        }

        [Key]
        public int Id { get; set; }

        public String Name { get; set; }
        [MaxLength(256)]
        public String Address { get; set; }

        [InverseProperty("EmailAddress")]
        public virtual List<RecipientDO> Recipients { get; set; }

        [InverseProperty("From")]
        public virtual List<EmailDO> SentEmails { get; set; }

        public String NameOrAddress()
        {
            if (!String.IsNullOrEmpty(Name))
                return Name;
            return Address;
        }

        
        public String ToDisplayName()
        {
            if (!String.IsNullOrEmpty(Name))
                return String.Format("<{0}> {1}", Name, Address);
            return Address;
        }
    }
}
