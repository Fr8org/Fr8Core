using System;
using System.Threading.Tasks;
using Fr8.TerminalBase.Interfaces;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Services
{
    public class GoogleGDrivePolling : IGoogleGDrivePolling
    {
        public async Task SchedulePolling(
            IHubCommunicator hubCommunicator,
            string externalAccountId,
            string gDriveFileId,
            GDrivePollingType pollingType,
            bool triggerImmediatly)
        {
            var pollingInterval = "1";
            await hubCommunicator.ScheduleEvent(
                externalAccountId,
                pollingInterval,
                triggerImmediatly,
                additionalConfigAttributes: gDriveFileId ?? null,
                additionToJobId: pollingType.ToString()
            );
        }
    }
}