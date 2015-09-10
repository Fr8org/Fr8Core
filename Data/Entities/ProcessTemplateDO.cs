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
            StartingProcessNodeTemplate = new ProcessNodeTemplateDO();
            ProcessNodeTemplates = new List<ProcessNodeTemplateDO>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string ProcessNodeTemplateOrdering { get; set; }

        public string Description { get; set; }

        [ForeignKey("StartingProcessNodeTemplate")]
        public int StartingProcessNodeTemplateId { get; set; }

        public virtual ProcessNodeTemplateDO StartingProcessNodeTemplate { get; set; }

        [Required]
        [ForeignKey("ProcessTemplateStateTemplate")]
        public int ProcessTemplateState { get; set; }

        public virtual _ProcessTemplateStateTemplate ProcessTemplateStateTemplate { get; set; }

        public virtual DockyardAccountDO DockyardAccount { get; set; }

        [InverseProperty("DocuSignProcessTemplate")]
        public virtual IList<DocuSignTemplateSubscriptionDO> SubscribedDocuSignTemplates { get; set; }

        [InverseProperty("ExternalProcessTemplate")]
        public virtual IList<ExternalEventSubscriptionDO> SubscribedExternalEvents { get; set; }

        [InverseProperty("ProcessTemplate")]
        public virtual ICollection<ProcessDO> ChildProcesses { get; set; }

        [InverseProperty("ProcessTemplate")]
        public virtual IList<ProcessNodeTemplateDO> ProcessNodeTemplates { get; set; }
    }
}