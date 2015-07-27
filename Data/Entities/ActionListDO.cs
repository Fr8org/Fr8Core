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
			this.ActionOrdering = new List< ActionDO >();
		}

		[ Key ]
		public int Id{ get; set; }

		public string Name{ get; set; }

		[ ForeignKey( "Template" ) ]
		public int? TemplateId{ get; set; }
		public virtual TemplateDO Template{ get; set; }

		[ ForeignKey( "Process" ) ]
		public int? ProcessID{ get; set; }
		public virtual ProcessDO Process{ get; set; }

		[ ForeignKey( "TriggerEvent" ) ]
		public int? TriggerEventID{ get; set; }

		public virtual _EventStatusTemplate TriggerEvent{ get; set; }

		[ InverseProperty( "ActionList" ) ]
		public List< ActionDO > ActionOrdering{ get; set; }
	}
}