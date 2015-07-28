using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
	public class ProcessNodeDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		public string Name{ get; set; }

		[ ForeignKey( "Process" ) ]
		public int ProcessID{ get; set; }

		public virtual ProcessDO Process{ get; set; }

		public ProcessNodeState State{ get; set; }

		public ProcessNodeTemplateDO ProcessNodeTemplate{ get; set; }

		public enum ProcessNodeState
		{
			Unstarted,
			EvaluatingCriteria,
			ProcessingActions,
			Complete,
			Error
		}
	}
}