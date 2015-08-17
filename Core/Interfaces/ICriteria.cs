using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Utilities;

namespace Core.Interfaces
{
    public interface ICriteria
    {
        bool Evaluate(string criteria, int processId, IEnumerable<EnvelopeData> envelopeData);

        bool Evaluate(EnvelopeDO curEnvelope, ProcessNodeDO curProcessNode);
    }
}