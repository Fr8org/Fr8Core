using System;
using System.Collections.Generic;

namespace Fr8Data.DataTransferObjects
{
    public class EmailDTO
    {
        public int Id { get; set; }
        public String MessageID { get; set; }
        public String References { get; set; }
        public String Subject { get; set; }
        public String HTMLText { get; set; }
        public String PlainText { get; set; }
        public DateTimeOffset DateReceived { get; set; }
        public int? EmailStatus { get; set; }
        public virtual EmailAddressDTO From { get; set; }
        public String FromName { get; set; }
        public String ReplyToName { get; set; }
        public String ReplyToAddress { get; set; }
        public virtual List<RecipientDTO> Recipients { get; set; }
    }
}
