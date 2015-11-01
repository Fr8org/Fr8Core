using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.Manifests;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IDockyardEvent
    {
        Task ProcessInboundEvents(CrateDTO curCrateStandardEventReport);
        Task LaunchProcess(RouteDO curRoute, CrateDTO curEventData = null);
        Task LaunchProcesses(List<RouteDO> curRoutes, CrateDTO curEventReport);
    }
}
