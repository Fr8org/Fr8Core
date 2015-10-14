using Data.States.Templates;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    /// <summary>
    /// Criteria object for ProcessNodeTemplate.
    /// </summary>
    public class CriteriaDO : BaseDO
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("ProcessNodeTemplate")]
        public int? ProcessNodeTemplateId { get; set; }

        /// <summary>
        /// Reference to parent ProcessTemplateNode.
        /// Every Criteria must belong to a single ProcessTemplateNode.
        /// </summary>
        public virtual ProcessNodeTemplateDO ProcessNodeTemplate { get; set; }
        

        //the criteria execution type reflects the radio button choice: apply criteria? or execute without worrying about the criteria?
        [Required]
        [ForeignKey("ExecutionTypeTemplate")]
        public int CriteriaExecutionType { get; set; }

        /// <summary>
        /// Execute when conditions are satisfied,      
        /// or execute regardless of conditions.
        /// </summary>
        public virtual _CriteriaExecutionTypeTemplate ExecutionTypeTemplate { get; set; }

        /// <summary>
        /// Conditions JSON string
        /// </summary>
        public string ConditionsJSON { get; set; }
    }
}
