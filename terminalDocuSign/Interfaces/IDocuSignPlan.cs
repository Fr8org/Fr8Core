using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;

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
        Task CreatePlan_MonitorAllDocuSignEvents(string curFr8UserId, AuthorizationTokenDTO authTokenDTO);

        void CreateConnect(string curFr8UserId, AuthorizationTokenDTO authTokenDTO);

        void CreateOrUpdatePolling(string curFr8UserId, AuthorizationTokenDTO authTokenDTO);

    }
}
