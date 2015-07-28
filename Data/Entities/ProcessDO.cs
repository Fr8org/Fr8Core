using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
	public class ProcessDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		public string Name{ get; set; }
		public string Description{ get; set; }
		public string UserId{ get; set; }
		public ProcessNodeTemplateDO StartingProcessNodeTemplate{ get; set; }

		[ Required ]
		[ ForeignKey( "ProcessStateTemplate" ) ]
		public int ProcessState{ get; set; }

		public virtual _ProcessStateTemplate ProcessStateTemplate{ get; set; }
	}
}