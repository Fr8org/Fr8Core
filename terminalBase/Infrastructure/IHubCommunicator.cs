using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.States;

namespace TerminalBase.Infrastructure
{
    public interface IHubCommunicator
    {
        Task<PayloadDTO> GetProcessPayload(Guid containerId);
        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(
            Guid routeNodeId, CrateDirection direction);
    }
}
