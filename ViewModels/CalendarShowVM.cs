using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class CalendarShowVM
    {
        public List<int> LinkedCalendarIds { get; set; }
        public int? LinkedNegotiationID { get; set; }
        public int ActiveCalendarId { get; set; }
        
    }

}