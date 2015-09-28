using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class EnvelopeDO  
    {
        [Key]
        public int Id { get; set; }

        public EnvelopeState EnvelopeStatus { get; set; } //renamed to envelopestatus because it will hide the parent status property
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