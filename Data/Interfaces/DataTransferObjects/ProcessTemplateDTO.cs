using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.Interfaces.DataTransferObjects
{
    public class ProcessTemplateDTO
    {
        public ProcessTemplateDTO()
        {
            SubscribedDocuSignTemplates = new List<DocuSignTemplateSubscriptionDO>();
            SubscribedExternalEvents = new List<ExternalEventSubscriptionDO>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public int ProcessTemplateState { get; set; }

        public virtual IList<DocuSignTemplateSubscriptionDO> SubscribedDocuSignTemplates { get; set; }

        public virtual IList<ExternalEventSubscriptionDO> SubscribedExternalEvents { get; set; }
    }
}