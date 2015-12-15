using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Interfaces.DataTransferObjects;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;

namespace terminalQuickBooks.Interfaces
{
    public interface IQuickBooksIntegration
    {
        string CreateAuthUrl();
        DataService GetDataService(AuthorizationTokenDTO authorizationTokenDto);
        Task<string> GetOAuthToken(string oauthToken, string oauthVerifier, string realmId);
        ServiceContext CreateServiceContext(string oauthToken);
    }
}