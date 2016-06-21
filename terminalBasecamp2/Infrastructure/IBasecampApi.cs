using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;
using terminalBasecamp.Data;

namespace terminalBasecamp.Infrastructure
{
    public interface IBasecampApi
    {
        ExternalAuthUrlDTO GetExternalAuthUrl();
        Task<AuthorizationTokenDTO> AuthenticateAsync(ExternalAuthenticationDTO externalState);
        Task<Authorization> GetCurrentUserInfo(AuthorizationToken authorizationToken);
        Task<List<Account>>  GetAccounts(AuthorizationToken authorizationToken);
    }
}