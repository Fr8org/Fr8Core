using System.Threading.Tasks;
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
            string gDriveFileId,
            GDrivePollingType pollingType,
            bool triggerImmediatly
        );
    }
}
