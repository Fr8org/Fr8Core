using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.Security;
using terminalQuickBooks.Interfaces;
using Utilities.Configuration.Azure;


namespace terminalQuickBooks.Services
{
    public class QuickBooksIntegration : IQuickBooksIntegration
    {
        public string CreateAuthUrl()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetOAuthToken(string oauthToken, string oauthVerifier, string realmId)
        {
            throw new NotImplementedException();
        }
    }
}