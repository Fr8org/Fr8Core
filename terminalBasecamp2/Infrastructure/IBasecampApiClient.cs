using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using terminalBasecamp2.Data;

namespace terminalBasecamp2.Infrastructure
{
    /// <summary>
    /// Provides wrapper around Basecamp REST API
    /// </summary>
    public interface IBasecampApiClient : IOAuthApiIntegration
    {
        /// <summary>
        /// Gets URL that is used by Basecamp to aks user for authorization
        /// </summary>
        ExternalAuthUrlDTO GetExternalAuthUrl();
        /// <summary>
        /// Performs OAuth authentication after receiving verification code from Basecamp
        /// </summary>
        Task<AuthorizationTokenDTO> AuthenticateAsync(ExternalAuthenticationDTO externalState);
        /// <summary>
        /// Gets basic information about authorized user along with accounts available to him
        /// </summary>
        Task<Authorization> GetCurrentUserInfo(AuthorizationToken auth);
        /// <summary>
        /// Gets accounts available to authorized user
        /// </summary>
        Task<List<Account>>  GetAccounts(AuthorizationToken auth);
        /// <summary>
        /// Get projects belong to specified account and available for authorized user
        /// </summary>
        Task<List<Project>> GetProjects(string accountUrl, AuthorizationToken auth);
        /// <summary>
        /// Post a new message into specified project of authorized user
        /// </summary>
        /// <returns>A message that has been posted</returns>
        Task<Message> CreateMessage(string accountUrl, string projectId, string subject, string content, AuthorizationToken auth);
    }
}