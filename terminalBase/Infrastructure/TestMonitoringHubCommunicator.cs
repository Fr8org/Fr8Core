using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.States;

namespace TerminalBase.Infrastructure
{
    public class TestMonitoringHubCommunicator : IHubCommunicator
    {
        public Task<PayloadDTO> GetProcessPayload(Guid containerId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(
            Guid routeNodeId, CrateDirection direction)
        {
            throw new NotImplementedException();
        }
    }
}
