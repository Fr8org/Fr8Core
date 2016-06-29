using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;
using terminalMailChimp.Models;

namespace terminalMailChimp.Interfaces
{
    public interface IMailChimpIntegration
    {
        ExternalAuthUrlDTO GenerateOAuthInitialUrl();
        Task<AuthorizationTokenDTO> GetAuthToken(string code, string state);
        Task<string> GetExternalUserId(object oauthToken);
        Task<List<MailChimpList>> GetLists(AuthorizationToken authorizationToken)
    }
}