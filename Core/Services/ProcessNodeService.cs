using Data.Entities;
using Data.Infrastructure;

namespace Core.Services
{
	public class ProcessNodeService
	{
		public void Create( UnitOfWork uow, ProcessDO parentProcess )
		{
			var node = new ProcessNodeDO
			{
				State = ProcessNodeDO.ProcessNodeState.Unstarted,
				ProcessStateTemplate = parentProcess.ProcessStateTemplate,
				ProcessID = parentProcess.Id
			};

			uow.ProcessNodeRepository.Add( node );
		}

		public void CreateTruthTransition( ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode )
		{
		}
	}
}