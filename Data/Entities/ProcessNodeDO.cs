using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
	public class ProcessNodeDO: BaseDO
	{
		[ Key ]
		public int Id{ get; set; }

		public string Name{ get; set; }

		public ProcessDO ParentProcess{ get; set; }

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