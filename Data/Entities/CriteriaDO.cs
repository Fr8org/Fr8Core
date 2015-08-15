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

        [Required]
        [ForeignKey("ProcessNodeTemplate")]
        public int ProcessNodeTemplateID { get; set; }

        /// <summary>
        /// Reference to parent ProcessTemplateNode.
        /// Every Criteria must belong to a single ProcessTemplateNode.
        /// </summary>
        public virtual ProcessNodeTemplateDO ProcessNodeTemplate { get; set; }

        [Required]
        [ForeignKey("ExecutionTypeTemplate")]
        public int ExecutionType { get; set; }

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
