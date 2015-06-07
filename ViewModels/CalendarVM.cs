using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class CalendarVM
    {
        public int BookingRequestID { get; set; }
        public int? LinkedNegotiationID { get; set; }
        public List<int> LinkedCalendarIDs { get; set; }
        public int ActiveCalendarID { get; set; }
    }
}