using Data.Entities;
using Data.Infrastructure;

namespace Core.Services
{
	public class ProcessNodeService
	{
		/// <summary>
		/// Creates ProcessNode Object
		/// </summary>
		/// <returns>New ProcessNodeDO instance</returns>
		public ProcessNodeDO Create( UnitOfWork uow, ProcessDO parentProcess )
		{
			var processNode = new ProcessNodeDO
			{
				State = ProcessNodeDO.ProcessNodeState.Unstarted,
				ProcessNodeTemplate = parentProcess.StartingProcessNodeTemplate,
				ParentProcess = parentProcess
			};

			uow.ProcessNodeRepository.Add( processNode );

			return processNode;
		}

		public void CreateTruthTransition( ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode )
		{
		}
	}
}