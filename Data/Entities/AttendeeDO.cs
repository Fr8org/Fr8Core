using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class AttendeeDO : BaseDO, IAttendeeDO
    {
        [Key]
        public int Id { get; set; }

        public String Name { get; set; }

        [ForeignKey("EmailAddress")]
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        [ForeignKey("Event")]
        public int? EventID { get; set; }
        public virtual EventDO Event { get; set; }

        [ForeignKey("Negotiation")]
        public int? NegotiationID { get; set; }
        public virtual NegotiationDO Negotiation { get; set; }

        [ForeignKey("ParticipationStatusTemplate"), DefaultValue(States.ParticipationStatus.NeedsAction)]
        public int? ParticipationStatus { get; set; }
        public _ParticipationStatusTemplate ParticipationStatusTemplate { get; set; }
    }
}
