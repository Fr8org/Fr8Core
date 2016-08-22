using System;
using System.Collections.Generic;
using Utilities;

namespace Web.ViewModels
{
    public class BookingRequestAdminVM
    {
        public BookingRequestAdminVM()
        {
            Conversations = new List<ConversationVM>();
        }

        public List<ConversationVM> Conversations { get; set; }
        public int BookingRequestId { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string EmailBCC { get; set; }
        public string EmailAttachments { get; set; }
        public string Booker { get; set; } 
        public bool ReadOnly { get; set; }
        public List<String> VerbalisedHistory { get; set; }
        public DateTimeOffset LastUpdated { get; set; }

        public String UserTimeZoneID { get; set; }
        public bool TimezoneGuessed { get; set; }

        public String FromUserID { get; set; }
    }

    public class ConversationVM
    {
        public String FromEmailAddress { get; set; }
        public String DateRecieved { get; set; }
        public String Body { get; set; }
        public bool ExplicitOpen { get; set; }
    }
}