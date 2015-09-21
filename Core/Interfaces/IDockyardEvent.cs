using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.ManifestSchemas;

namespace Core.Interfaces
{
    public interface IDockyardEvent
    {
        void ProcessInbound(string userID, EventReportMS curEventReport);
    }
}
