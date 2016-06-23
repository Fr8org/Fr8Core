using System.Threading.Tasks;
using Fr8.TerminalBase.Interfaces;

namespace terminalStatX.Interfaces
{
    public interface IStatXPolling
    {
        void SchedulePolling(IHubCommunicator hubCommunicator, string externalAccountId);
        Task<bool> Poll(IHubCommunicator hubCommunicator, string externalAccountId, string pollingInterval);
    }
}
