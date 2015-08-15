using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
	public interface IProcessNode
	{
		ProcessNodeDO Create(IUnitOfWork uow, ProcessDO parentProcess, string name);
		void CreateTruthTransition(ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode);
		void Execute(EnvelopeDO curEnvelope, ProcessNodeDO curProcessNode);
	}
}