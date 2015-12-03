using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace terminalQuickBooks.Interfaces
{
    public interface IQuickBooksIntegration
    {
        string CreateAuthUrl();
        Task<string> GetOAuthToken(string oauthToken, string oauthVerifier, string realmId);
    }
}