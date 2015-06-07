using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class InvitationDO : EmailDO
    {
        [ForeignKey("InvitationTypeTemplate")]
        public int? InvitationType { get; set; }
        public _InvitationTypeTemplate InvitationTypeTemplate { get; set; }

        [Required, ForeignKey("ConfirmationStatusTemplate")]
        public int? ConfirmationStatus { get; set; }
        public virtual _ConfirmationStatusTemplate ConfirmationStatusTemplate { get; set; }
    }
}
