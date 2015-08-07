using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class EnvelopeDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        public EnvelopeState Status { get; set; }
        public string DocusignEnvelopeId { get; set; }

        public enum EnvelopeState
        {
            Any,
            Created,
            Sent,
            Delivered,
            Signed,
            Completed,
            Declined,
            Voided,
            Deleted
        };
    }
}