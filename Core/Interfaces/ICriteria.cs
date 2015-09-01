using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
	public interface ICriteria
	{
		bool Evaluate(string criteria, int processId, IEnumerable<EnvelopeDataDTO> envelopeData);
		bool Evaluate(List<EnvelopeDataDTO> envelopeData, ProcessNodeDO curProcessNode);
		IQueryable<EnvelopeDataDTO> Filter(string criteria, int processId, IQueryable<EnvelopeDataDTO> envelopeData);
	}
}