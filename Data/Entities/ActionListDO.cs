using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class ActionListDO : ActivityDO /*, IActionListChild*/
	{
		public ActionListDO()
		{
            Activities = new List<ActivityDO>();
		}

        // [ForeignKey("ParentActionList")]
        // public int? ParentActionListId { get; set; }
        // public virtual ActionListDO ParentActionList { get; set; }

 		public string Name { get; set; }

        [ForeignKey("ProcessNodeTemplate")]
        [Column("ProcessNodeTemplateDO_Id")]
        public int? ProcessNodeTemplateID { get; set; }
        public virtual ProcessNodeTemplateDO ProcessNodeTemplate { get; set; }


		[ForeignKey("Process")]
		public int? ProcessID{ get; set; }
		public virtual ProcessDO Process{ get; set; }

		[ForeignKey("TriggerEvent")]
		public int? TriggerEventID{ get; set; }

		public virtual _ExternalEventTypeTemplate TriggerEvent{ get; set; }

        // [InverseProperty("ParentActionList")]
        // public virtual List<ActionListDO> ActionLists { get; set; }

        [Required]
        [ForeignKey("ActionListTypeTemplate")]
        public int ActionListType { get; set; }
        public virtual _ActionListTypeTemplate ActionListTypeTemplate { get; set; }

        [ForeignKey("CurrentActivity")]
        public int? CurrentActivityID { get; set; }
        public virtual ActivityDO CurrentActivity { get; set; }

        [ForeignKey("ActionListStateTemplate")]
        public int? ActionListState { get; set; }

        public virtual _ActionListStateTemplate ActionListStateTemplate { get; set; }
	}
}