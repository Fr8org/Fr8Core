using System.Collections.Generic;

namespace Web.ViewModels
{
    public class CalendarShowVM
    {
        public List<int> LinkedCalendarIds { get; set; }
        public int? LinkedNegotiationID { get; set; }
        public int ActiveCalendarId { get; set; }
        
    }

}