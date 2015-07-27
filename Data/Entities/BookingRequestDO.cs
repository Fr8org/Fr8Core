using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Data.States.Templates;
using Utilities;

namespace Data.Entities
{
    public class BookingRequestDO : EmailDO
    {
        public BookingRequestDO()
        {
            Calendars = new List<CalendarDO>();
            Negotiations = new List<NegotiationDO>();
            Availability = BookingRequestAvailability.Available;
            State = BookingRequestState.NeedsBooking;
            ConversationMembers = new List<EmailDO>();
        }

        [Required, ForeignKey("Customer")]
        public string CustomerID { get; set; }        
        public virtual DockyardAccountDO Customer { get; set; }

        [Required, ForeignKey("BookingRequestStateTemplate")]
        public int? State { get; set; }
        public virtual _BookingRequestStateTemplate BookingRequestStateTemplate { get; set; }

        [ForeignKey("Booker")]
        public string BookerID { get; set; }
        public virtual DockyardAccountDO Booker { get; set; }

        [ForeignKey("PreferredBooker")]
        public string PreferredBookerID { get; set; }
        public virtual DockyardAccountDO PreferredBooker { get; set; }

        [ForeignKey("BookingRequestAvailabilityTemplate")]
        public int? Availability { get; set; }
        public virtual _BookingRequestAvailabilityTemplate BookingRequestAvailabilityTemplate { get; set; }

        //Do not add InverseProperty - The relationship is handled in KwasantDbContext
        public virtual List<InstructionDO> Instructions { get; set; }

        [InverseProperty("BookingRequest")]
        public virtual IList<NegotiationDO> Negotiations { get; set; }

        [InverseProperty("BookingRequests")]
        public virtual List<CalendarDO> Calendars { get; set; }

        [InverseProperty("Conversation")]
        public virtual List<EmailDO> ConversationMembers { get; set; }
        
        public override void AfterCreate()
        {
            AlertManager.BookingRequestCreated(Id);
            base.AfterCreate();
        }

        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            base.OnModify(originalValues, currentValues);

            var reflectionHelper = new ReflectionHelper<BookingRequestDO>();
            var statePropertyName = reflectionHelper.GetPropertyName(br => br.State);
            if (!MiscUtils.AreEqual(originalValues[statePropertyName], currentValues[statePropertyName]))
            {
                var state = (int) currentValues[statePropertyName];
                switch (state)
                {
                    case BookingRequestState.NeedsBooking:
                        AlertManager.BookingRequestNeedsProcessing(Id);
                        break;
                    case BookingRequestState.Resolved:
                        AlertManager.BookingRequestMarkedProcessed(Id, BookerID);
                        break;
                }
            }
        }

       
    }
}
