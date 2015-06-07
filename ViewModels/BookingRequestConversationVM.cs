using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;

namespace KwasantWeb.ViewModels
{
    public class BookingRequestConversationVM  
    {
        public List<String> FromAddress { get; set; }

        public List<String> DateReceived { get; set; }

        public List<int> ConversationMembers { get; set; }

        public List<String> HTMLText { get; set; }

        public int? CurEmailId { get; set; }
    }
}