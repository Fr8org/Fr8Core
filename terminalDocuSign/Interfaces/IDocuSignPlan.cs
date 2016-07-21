using System.Threading.Tasks;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace terminalDocuSign.Interfaces
{
    /// <summary>
    /// Service to create DocuSign related plans in Hub
    /// </summary>
    public interface IDocuSignPlan
    {
        /// <summary>
        /// Creates Monitor All DocuSign Events plan with Record DocuSign Events and Store MT Data actions.
        /// </summary>
        Task CreatePlan_MonitorAllDocuSignEvents(IHubCommunicator hubCommunicator, AuthorizationToken authToken);

        void CreateConnect(IHubCommunicator hubCommunicator, AuthorizationToken authToken);

        void CreateOrUpdatePolling(IHubCommunicator hubCommunicator, AuthorizationToken authToken);

    }
}
