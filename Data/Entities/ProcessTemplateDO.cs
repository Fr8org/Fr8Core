using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
	public class ProcessTemplateDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		[ Required ]
		public string Name{ get; set; }

		public string ProcessNodeTemplateOrdering{ get; set; }

		public string Description{ get; set; }

		[ Required ]
		[ ForeignKey( "ProcessTemplateStateTemplate" ) ]
		public int ProcessTemplateState  { get; set; }

		public virtual _ProcessTemplateStateTemplate ProcessTemplateStateTemplate{ get; set; }
	}
}