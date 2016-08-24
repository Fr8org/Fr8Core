using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;

namespace terminalGoogle.Interfaces
{
    public enum GDrivePollingType
    {
        Spreadsheets
    }

    public interface IGoogleGDrivePolling
    {
        Task SchedulePolling(
            IHubCommunicator hubCommunicator,
            string externalAccountId,
            GDrivePollingType pollingType,
            bool triggerImmediatly
        );

        Task<PollingDataDTO> Poll(PollingDataDTO pollingData, GDrivePollingType pollingType);
    }
}
