using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;

namespace Fr8.TerminalBase.Services
{
    public interface IHubEventReporter
    {
        TerminalDTO Terminal { get; }

        Task Broadcast(Crate eventPayload);
        Task<IHubCommunicator> GetMasterHubCommunicator();
    }
}