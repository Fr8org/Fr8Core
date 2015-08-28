using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Data.Entities
{
    public class ExternalEventSubscriptionDO : BaseDO, IEquatable<ExternalEventSubscriptionDO>
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("EventStatusTemplate")]
        public int ExternalEvent { get; set; }

        public virtual _EventStatusTemplate EventStatusTemplate { get; set; }

        [ForeignKey("ExternalProcessTemplate")]
        public int ExternalProcessTemplateId { get; set; }

        public virtual ProcessTemplateDO ExternalProcessTemplate { get; set; }

        public bool Equals(ExternalEventSubscriptionDO other)
        {
            return ExternalProcessTemplateId == other.ExternalProcessTemplateId && ExternalEvent == other.ExternalEvent;
        }
    }
}