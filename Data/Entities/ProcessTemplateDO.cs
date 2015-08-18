using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class ProcessTemplateDO : BaseDO
    {
        public ProcessTemplateDO()
        {
            SubscribedDocuSignTemplates = new List<DocuSignTemplateSubscriptionDO>();
            SubscribedExternalEvents = new List<ExternalEventSubscriptionDO>();
            DockyardAccount = new DockyardAccountDO();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string ProcessNodeTemplateOrdering { get; set; }

        public string Description { get; set; }

        public int StartingProcessNodeTemplate { get; set; }

        [Required]
        [ForeignKey("ProcessTemplateStateTemplate")]
        public int ProcessTemplateState { get; set; }

        public virtual _ProcessTemplateStateTemplate ProcessTemplateStateTemplate { get; set; }

        public virtual DockyardAccountDO DockyardAccount { get; set; }

        [InverseProperty("ProcessTemplate")]
        public virtual IList<DocuSignTemplateSubscriptionDO> SubscribedDocuSignTemplates { get; set; }

        [InverseProperty("ProcessTemplate")]
        public virtual IList<ExternalEventSubscriptionDO> SubscribedExternalEvents { get; set; }
    }
}