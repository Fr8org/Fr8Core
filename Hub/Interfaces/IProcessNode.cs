using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
	public interface IProcessNode
	{
	    ProcessNodeDO Create(IUnitOfWork uow, Guid parentProcessId, Guid subrouteId, string name);

        void CreateTruthTransition(ProcessNodeDO sourcePNode, ProcessNodeDO targetPNode);

        //Obsolete
        //string Execute(List<EnvelopeDataDTO> curEnvelopeData, ProcessNodeDO curProcessNode);
	}
}