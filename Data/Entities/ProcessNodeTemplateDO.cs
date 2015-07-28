using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States;
using Data.States.Templates;

namespace Data.Entities
{
	public class ProcessNodeTemplateDO: IStateTemplate< ProcessTemplateState >
	{
		public ProcessNodeTemplateDO()
		{
			this.ActionLists = new List< ActionListDO >();
		}

		[ Key ]
		[ DatabaseGenerated( DatabaseGeneratedOption.None ) ]
		public int Id{ get; set; }

		public string Name{ get; set; }

		[ ForeignKey( "ProcessTemplate" ) ]
		public int? ProcessTemplateID{ get; set; }
		public virtual ProcessTemplateDO ProcessTemplate{ get; set; }

		public string TransitionKey{ get; set; }

		public List< ActionListDO > ActionLists{ get; set; }

		public override string ToString()
		{
			return this.Name;
		}
	}
}