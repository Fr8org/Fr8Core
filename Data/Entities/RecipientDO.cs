using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class RecipientDO : BaseObject
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Email")]
        public int? EmailID { get; set; }
        public virtual EmailDO Email { get; set; }

        [ForeignKey("EmailAddress")]
        public int? EmailAddressID { get; set; }
        public virtual EmailAddressDO EmailAddress { get; set; }

        [ForeignKey("EmailParticipantTypeTemplate")]
        public int? EmailParticipantType { get; set; }
        public _EmailParticipantTypeTemplate EmailParticipantTypeTemplate { get; set; }
    }
}
