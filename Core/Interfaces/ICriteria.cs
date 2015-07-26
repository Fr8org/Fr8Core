using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Core.Interfaces
{
    public interface ICriteria
    {
        bool Evaluate(string criteria, int processId, string envelopeId, IEnumerable<EnvelopeDataDO> envelopeData);
    }
}