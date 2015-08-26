using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
	public class ActionListDO: BaseDO
	{
		public ActionListDO()
		{
			Actions = new List<ActionDO>();
		}

		[Key]
		public int Id{ get; set; }

		public string Name{ get; set; }

        [ForeignKey("ProcessNodeTemplate")]
        [Column("ProcessNodeTemplateDO_Id")]
        public int? ProcessNodeTemplateID { get; set; }
        public virtual ProcessNodeTemplateDO ProcessNodeTemplate { get; set; }


		[ForeignKey("Process")]
		public int? ProcessID{ get; set; }
		public virtual ProcessDO Process{ get; set; }

		[ForeignKey("TriggerEvent")]
		public int? TriggerEventID{ get; set; }

		public virtual _EventStatusTemplate TriggerEvent{ get; set; }

		[InverseProperty("ActionList")]
		public virtual List<ActionDO> Actions{ get; set; }

        [Required]
        [ForeignKey("ActionListTypeTemplate")]
        public int ActionListType { get; set; }
        public virtual _ActionListTypeTemplate ActionListTypeTemplate { get; set; }

        [ForeignKey("CurrentAction")]
        public int? CurrentActionID { get; set; }
        public virtual ActionDO CurrentAction  { get; set; }

        [ForeignKey("ActionListStateTemplate")]
        public int? ActionListState { get; set; }

        public virtual _ActionListStateTemplate ActionListStateTemplate { get; set; }
	}
}