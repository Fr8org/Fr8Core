using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
	public interface IProcessNodeService
	{
		ProcessNodeDO Create( IUnitOfWork uow, ProcessDO parentProcess );
		void CreateTruthTransition( ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode );
	}
}