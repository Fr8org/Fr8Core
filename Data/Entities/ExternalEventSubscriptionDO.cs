using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Data.Entities
{
    public class ExternalEventSubscriptionDO : IEquatable<ExternalEventSubscriptionDO>
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

        public bool Equals(ExternalEventSubscriptionDO other)
        {
            return ProcessTemplateId == other.ProcessTemplateId && ExternalEvent == other.ExternalEvent;
        }
    }
}