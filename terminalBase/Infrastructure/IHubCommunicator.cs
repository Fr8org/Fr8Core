using System;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;

namespace TerminalBase.Infrastructure
{
    public interface IHubCommunicator
    {
        Task<PayloadDTO> GetProcessPayload(Guid containerId);
    }
}
