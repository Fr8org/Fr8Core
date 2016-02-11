using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;

namespace terminalDocuSign.Interfaces
{
    /// <summary>
    /// Service to create DocuSign related routes in Hub
    /// </summary>
    public interface IDocuSignRoute
    {
        /// <summary>
        /// Creates Monitor All DocuSign Events plan with Record DocuSign Events and Store MT Data actions.
        /// </summary>
        Task CreateRoute_MonitorAllDocuSignEvents(string curFr8UserId, AuthorizationTokenDTO authTokenDTO);
    }
}
