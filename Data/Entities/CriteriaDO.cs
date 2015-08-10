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
