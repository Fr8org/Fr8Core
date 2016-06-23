using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using terminalBasecamp2.Data;

namespace terminalBasecamp2.Infrastructure
{
    public interface IBasecampApiClient : IOAuthApiIntegration
    {
        ExternalAuthUrlDTO GetExternalAuthUrl();
        Task<AuthorizationTokenDTO> AuthenticateAsync(ExternalAuthenticationDTO externalState);
        Task<Authorization> GetCurrentUserInfo(AuthorizationToken auth);
        Task<List<Account>>  GetAccounts(AuthorizationToken auth);
        Task<List<Project>> GetProjects(string accountUrl, AuthorizationToken auth);
        Task<Message> CreateMessage(string accountUrl, string projectId, string subject, string content, AuthorizationToken auth);
    }
}