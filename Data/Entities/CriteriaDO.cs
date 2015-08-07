using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class CriteriaDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ProcessTemplate")]
        public int ProcessTemplateId { get; set; }

        /// <summary>
        /// ProcessTemplate to which Criteria belongs to.
        /// </summary>
        public virtual ProcessTemplateDO ProcessTemplate { get; set; }

        /// <summary>
        /// Criteria name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Execute when conditions are satisfied,
        /// or execute regardless of conditions.
        /// </summary>
        public CriteriaExecutionMode ExecutionMode { get; set; }

        /// <summary>
        /// Conditions JSON string
        /// </summary>
        public string ConditionsJSON { get; set; }
    }
}
