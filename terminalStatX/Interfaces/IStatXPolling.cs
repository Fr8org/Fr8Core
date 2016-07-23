using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;

namespace terminalStatX.Interfaces
{
    public interface IStatXPolling
    {
        Task SchedulePolling(IHubCommunicator hubCommunicator, string externalAccountId, bool triggerImmediatly, string groupId, string statId);
        Task<PollingDataDTO> Poll(PollingDataDTO pollingData);
    }
}
