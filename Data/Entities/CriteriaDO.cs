using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    /// <summary>
    /// Criteria object for Subroute.
    /// </summary>
    public class CriteriaDO : BaseObject
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Subroute")]
        public Guid? SubrouteId { get; set; }

        /// <summary>
        /// Reference to parent RouteNode.
        /// Every Criteria must belong to a single RouteNode.
        /// </summary>
        public virtual SubrouteDO Subroute { get; set; }
        

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
