using System.Threading.Tasks;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using terminalStatX.Interfaces;

namespace terminalStatX.Services
{
    public class StatXPolling : IStatXPolling
    {
        public void SchedulePolling(IHubCommunicator hubCommunicator, string externalAccountId)
        {
            string pollingInterval = CloudConfigurationManager.GetSetting("terminalStatX.PollingInterval");
            hubCommunicator.ScheduleEvent(externalAccountId, pollingInterval);
        }

        public Task<bool> Poll(IHubCommunicator hubCommunicator, string externalAccountId, string pollingInterval)
        {
            throw new System.NotImplementedException();
        }
    }
}