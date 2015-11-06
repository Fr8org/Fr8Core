using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.Manifests;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IDockyardEvent
    {
        Task ProcessInboundEvents(Crate curCrateStandardEventReport);
        Task LaunchProcess(RouteDO curRoute, Crate curEventData = null);
        Task LaunchProcesses(List<RouteDO> curRoutes, Crate curEventReport);
    }
}
