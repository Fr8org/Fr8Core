using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;
using terminalMailChimp.Models;

namespace terminalMailChimp.Interfaces
{
    public interface IMailChimpIntegration
    {
        ExternalAuthUrlDTO GenerateOAuthInitialUrl();
        Task<AuthorizationTokenDTO> GetAuthToken(string code, string state);
        Task<List<MailChimpList>> GetLists(AuthorizationToken authorizationToken);
        Task CreateList(AuthorizationToken authorizationToken, MailChimpList mailChimpList);
        Task UpdateListWithNewSubscriber(AuthorizationToken authorizationToken, Subscriber subscriber);
    }
}