using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalQuickBooks.Interfaces;
using Utilities.Configuration.Azure;

namespace terminalQuickBooks.Services
{
    public class QuickBooksIntegration : IQuickBooksIntegration
    {
        private static readonly ConcurrentDictionary<string, string> TokenSecrets = new ConcurrentDictionary<string, string>(); 
        /// <summary>
        /// Build external QuickBooks OAuth url.
        /// </summary>
        public string CreateAuthUrl()
        {
            var oauthSession = CreateSession();
            var requestToken = oauthSession.GetRequestToken();
            TokenSecrets.TryAdd(requestToken.Token, requestToken.TokenSecret);
            return oauthSession.GetUserAuthorizationUrlForToken(requestToken);
        }

        /// <summary>
        /// Create a session.
        /// </summary>
        /// <returns></returns>
        private IOAuthSession CreateSession()
        {
            var consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = CloudConfigurationManager.GetSetting("QuickBooksConsumerKey").ToString(CultureInfo.InvariantCulture),
                ConsumerSecret = CloudConfigurationManager.GetSetting("QuickBooksConsumerSecret").ToString(CultureInfo.InvariantCulture),
                SignatureMethod = SignatureMethod.HmacSha1
            };
            return new OAuthSession(consumerContext,
                                    CloudConfigurationManager.GetSetting("QuickBooksRequestTokenUrl").ToString(CultureInfo.InvariantCulture),
                                    CloudConfigurationManager.GetSetting("QuickBooksOAuthAuthorizeUrl").ToString(CultureInfo.InvariantCulture),
                                    CloudConfigurationManager.GetSetting("QuickBooksOAuthAccessUrl").ToString(CultureInfo.InvariantCulture));
        }

        public async Task<string> GetOAuthToken(string oauthToken, string oauthVerifier)
        {
            var oauthSession = CreateSession();
            string tokenSecret;
            TokenSecrets.TryGetValue(oauthToken, out tokenSecret);

            IToken reqToken = new RequestToken
            {
                Token = oauthToken,
                TokenSecret = tokenSecret,
                ConsumerKey = CloudConfigurationManager.GetSetting("QuickBooksConsumerKey").ToString(CultureInfo.InvariantCulture)
            };
            var accToken = oauthSession.ExchangeRequestTokenForAccessToken(reqToken, oauthVerifier);

            return accToken.Token;
        }

    }
}