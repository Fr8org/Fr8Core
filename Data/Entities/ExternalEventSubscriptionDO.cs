using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ExternalEventSubscriptionDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("EventStatusTemplate")]
        public int ExternalEvent { get; set; }

        public string DocuSignTemplateId { get; set; }

        public virtual _EventStatusTemplate EventStatusTemplate { get; set; }

        [ForeignKey("ProcessTemplate")]
        public int ProcessTemplateId { get; set; }

        public virtual ProcessTemplateDO ProcessTemplate { get; set; }
    }
}