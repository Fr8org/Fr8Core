using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{    
    public class CalendarDO : BaseDO, ICalendarDO
    {
        public CalendarDO()
        {
            Events = new List<EventDO>();
            BookingRequests = new List<BookingRequestDO>();
        }

        [Key]
        public int Id { get; set; }
        
        public String Name { get; set; }

        [ForeignKey("Owner")]
        public string OwnerID { get; set; }
        public virtual DockyardAccountDO Owner { get; set; }

        [ForeignKey("Negotiation")]
        public int? NegotiationID { get; set; }
        public virtual NegotiationDO Negotiation { get; set; }

        [InverseProperty("Calendars")]
        public virtual List<BookingRequestDO> BookingRequests { get; set; }

        [InverseProperty("Calendar")]
        public virtual List<EventDO> Events { get; set; }
    }
}
