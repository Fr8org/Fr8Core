using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using System.Linq;
using System;

namespace Data.Entities
{
    public class ProcessTemplateDO : BaseDO
    {
        public ProcessTemplateDO()
        {
            SubscribedDocuSignTemplates = new List<DocuSignTemplateSubscriptionDO>();
            SubscribedExternalEvents = new List<ExternalEventSubscriptionDO>();

            
            ProcessNodeTemplates = new List<ProcessNodeTemplateDO>();
            /*var startingProcessNodeTemplate = new ProcessNodeTemplateDO();
            startingProcessNodeTemplate.StartingProcessNodeTemplate = true;
            ProcessNodeTemplates.Add(startingProcessNodeTemplate);*/
        }


        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string ProcessNodeTemplateOrdering { get; set; }

        public string Description { get; set; }

        /*[ForeignKey("StartingProcessNodeTemplate")]
        public int StartingProcessNodeTemplateId { get; set; }

        public virtual ProcessNodeTemplateDO StartingProcessNodeTemplate { get; set; }*/

        [NotMapped]
        public int StartingProcessNodeTemplateId
        {
            get
            {
                var startingProcessNodeTemplate = ProcessNodeTemplates.SingleOrDefault(pnt => pnt.StartingProcessNodeTemplate == true);
                if (null != startingProcessNodeTemplate)
                    return startingProcessNodeTemplate.Id;
                else
                {
                    return 0;
                    //throw new ApplicationException("Starting ProcessNodeTemplate doesn't exist.");
                }
            }
        }

        [NotMapped]
        public ProcessNodeTemplateDO StartingProcessNodeTemplate
        {
            get
            {
                return ProcessNodeTemplates.SingleOrDefault(pnt => pnt.StartingProcessNodeTemplate == true);
            }

            set {
                var startingProcessNodeTemplate = ProcessNodeTemplates.SingleOrDefault(pnt => pnt.StartingProcessNodeTemplate == true);
                if (null != startingProcessNodeTemplate)
                    startingProcessNodeTemplate = value;
                else
                {
                    ProcessNodeTemplates.ToList().ForEach(pnt => pnt.StartingProcessNodeTemplate = false);
                    value.StartingProcessNodeTemplate = true;
                    ProcessNodeTemplates.Add(value);
                }
            }
        }

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