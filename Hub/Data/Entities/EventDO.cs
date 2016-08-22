using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using System.Reflection;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Data.States.Templates;

namespace Data.Entities
{
    public class EventDO : BaseDO, IDeleteHook
    {
        public EventDO()
        {
            CreateType = EventCreateType.KwasantBR;
            SyncStatus = EventSyncState.DoNotSync;
            Attendees = new List<AttendeeDO>();
            Emails = new List<EmailDO>();
            ExternalGUID = Guid.NewGuid().ToString();
        }

        [NotMapped]
        public string ActivityStatus
        {
            get { return String.Empty; }
            set { throw new Exception("This field is reserved. You probably want to use 'State' instead."); }
        }

        [Key]
        public int Id { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string Location { get; set; }
        public string Transparency { get; set; }
        public string Class { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public int Sequence { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public bool IsAllDay { get; set; }
        public string ExternalGUID { get; set; }

        [ForeignKey("EventStatusTemplate")]
        public int? EventStatus { get; set; }
        public virtual _EventStatusTemplate EventStatusTemplate { get; set; }

        [ForeignKey("CreatedBy"), Required]
        public string CreatedByID { get; set; }
        public virtual DockyardAccountDO CreatedBy { get; set; }

        [ForeignKey("Calendar"), Required]
        public int? CalendarID { get; set; }
        public virtual CalendarDO Calendar { get; set; }

        [ForeignKey("BookingRequest")]
        public int? BookingRequestID { get; set; }
        public virtual BookingRequestDO BookingRequest { get; set; }
        
        [ForeignKey("CreateTypeTemplate"), Required]
        public int? CreateType { get; set; }
        public virtual _EventCreateTypeTemplate CreateTypeTemplate { get; set; }

        [ForeignKey("SyncStatusTemplate"), Required]
        public int? SyncStatus { get; set; }
        public virtual _EventSyncStatusTemplate SyncStatusTemplate { get; set; }

        [InverseProperty("Event")]
        public virtual List<AttendeeDO> Attendees { get; set; }

        [InverseProperty("Events")]
        public virtual List<EmailDO> Emails { get; set; }

        public void CopyFrom(EventDO eventDO)
        {
            //We can't called GetType() because EF mocks our object
            PropertyInfo[] props = typeof(EventDO).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(this, prop.GetValue(eventDO));
            }
        }

		public override void BeforeSave()        
        {
            base.BeforeSave();
            SetBookingRequestLastUpdated();
        }
        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            base.OnModify(originalValues, currentValues);
            SetBookingRequestLastUpdated();
        }

        public void OnDelete(DbPropertyValues originalValues)
        {
            SetBookingRequestLastUpdated();
        }

        private void SetBookingRequestLastUpdated()
        {
            var br = BookingRequest;
            if (br != null)
                br.LastUpdated = DateTime.Now;
        }
    }
}
