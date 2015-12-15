using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Intuit.Ipp.Core;

namespace terminalQuickBooks.Interfaces
{
    public interface IQuickBooksIntegration
    {
        string CreateAuthUrl();
        Task<string> GetOAuthToken(string oauthToken, string oauthVerifier, string realmId);
        ServiceContext CreateServiceContext(string oauthToken);
    }
}