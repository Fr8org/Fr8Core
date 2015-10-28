using System;
using System.Web.Mvc;

namespace HubWeb.ViewModels
{
    public class EventVM
    {
        public bool IsNew { get; set; }

        public int Id { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public String Location { get; set; }
        public String Status { get; set; }
        public String Class { get; set; }
        public String Description { get; set; }
        public int Priority { get; set; }
        
        public int Sequence { get; set; }
        public String Summary { get; set; }
        public String Category { get; set; }
        public int? BookingRequestID { get; set; }
        
        [AllowHtml]
        public String Attendees { get; set; }
        public String CreatedByAddress { get; set; }
        public String CreatedByID { get; set; }

        public Double BookingRequestTimezoneOffsetInMinutes { get; set; }
    }
}
