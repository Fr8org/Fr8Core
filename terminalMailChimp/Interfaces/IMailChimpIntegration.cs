using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalMailChimp.Interfaces
{
    public interface IMailChimpIntegration
    {
        ExternalAuthUrlDTO GenerateOAuthInitialUrl();
        Task<string> GetToken(string code);
        Task<string> GetExternalUserId(object oauthToken);
    }
}