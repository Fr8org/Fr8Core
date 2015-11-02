using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
	public interface IProcessNode
	{
	    ProcessNodeDO Create(IUnitOfWork uow, int parentProcessId, int subrouteId, string name);

        void CreateTruthTransition(ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode);
		string Execute(List<EnvelopeDataDTO> curEnvelopeData, ProcessNodeDO curProcessNode);
	}
}