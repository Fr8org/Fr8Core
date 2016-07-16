using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalGoogle.DataTransferObjects;

namespace terminalGoogle.Interfaces
{
    public interface IGoogleGmailPolling
    {
        Task<PollingDataDTO> Poll(PollingDataDTO pollingData);

        Task SchedulePolling(IHubCommunicator hubCommunicator, string externalAccountId, bool trigger_immediatly);
    }
}