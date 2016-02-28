using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    [Obsolete]
	public interface ICriteria
	{
		//bool Evaluate(string criteria, Guid processId, IEnumerable<EnvelopeDataDTO> envelopeData);
		//bool Evaluate(List<EnvelopeDataDTO> envelopeData, ProcessNodeDO curProcessNode);
		//IQueryable<EnvelopeDataDTO> Filter(string criteria, Guid processId, IQueryable<EnvelopeDataDTO> envelopeData);
	}
}