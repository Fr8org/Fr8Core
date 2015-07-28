using Data.Entities;
using Data.Infrastructure;

namespace Core.Interfaces
{
	public interface IProcessNodeService
	{
		ProcessNodeDO Create( UnitOfWork uow, ProcessDO parentProcess );
		void CreateTruthTransition( ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode );
	}
}