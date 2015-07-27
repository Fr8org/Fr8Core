using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class NegotiationAnswerEmailDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Email"), Required]
        public int? EmailID { get; set; }
        public virtual EmailDO Email { get; set; }

        [ForeignKey("Negotiation"), Required]
        public int? NegotiationID { get; set; }
        public virtual NegotiationDO Negotiation { get; set; }

        [ForeignKey("User"), Required]
        public String UserID { get; set; }
        public virtual DockyardAccountDO User { get; set; }
    }
}
