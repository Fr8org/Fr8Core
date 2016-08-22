using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Models;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Infrastructure
{
    public class Authenticator : IAuthenticator
    {
        public const string TokenSeparator = ";;;;;;;";

        public static readonly string AccessToken = CloudConfigurationManager.GetSetting("QuickBooksAppToken");

        public static readonly string RequestUrl = CloudConfigurationManager.GetSetting("QuickBooksRequestTokenUrl");

        public static readonly ConcurrentDictionary<string, string> TokenSecrets = new ConcurrentDictionary<string, string>();

        public static readonly string ConsumerKey = CloudConfigurationManager.GetSetting("QuickBooksConsumerKey");

        public static readonly string ConsumerSecret = CloudConfigurationManager.GetSetting("QuickBooksConsumerSecret");

        /// <summary>
        /// Build external QuickBooks OAuth url.
        /// </summary>
        /// <param name="state"></param>
        public string CreateAuthUrl(Guid state)
        {
            var oauthSession = CreateSession(state.ToString());
            var requestToken = oauthSession.GetRequestToken();
            TokenSecrets.TryAdd(requestToken.Token, requestToken.TokenSecret);
            return $"{oauthSession.GetUserAuthorizationUrlForToken(requestToken)}&state={state}";
        }

        /// <summary>
        /// Create a session.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private IOAuthSession CreateSession(string state)
        {
            var consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                SignatureMethod = SignatureMethod.HmacSha1
            };

            return new OAuthSession(
                consumerContext,
                RequestUrl,
                $"{CloudConfigurationManager.GetSetting("QuickBooksOAuthAuthorizeUrl")}%26state%3D{state}",
                CloudConfigurationManager.GetSetting("QuickBooksOAuthAccessUrl"));
        }

        public async Task<AuthorizationTokenDTO> GetAuthToken(string oauthToken, string oauthVerifier, string realmId, string state)
        {
            var oauthSession = CreateSession(state);
            string tokenSecret;
            TokenSecrets.TryRemove(oauthToken, out tokenSecret);

            IToken reqToken = new RequestToken
            {
                Token = oauthToken,
                TokenSecret = tokenSecret,
                ConsumerKey = CloudConfigurationManager.GetSetting("QuickBooksConsumerKey")
            };
            var accToken = oauthSession.ExchangeRequestTokenForAccessToken(reqToken, oauthVerifier);
            var expiresAt = DateTime.UtcNow.Date.AddDays(180);

            return new AuthorizationTokenDTO()
            {
                Token = string.Join(TokenSeparator, accToken.Token, accToken.TokenSecret, realmId, expiresAt),
                ExternalAccountId = realmId,
                ExternalStateToken = state,
                ExpiresAt = expiresAt,
            };
        }

        public Task<AuthorizationToken> RefreshAuthToken(AuthorizationToken authorizationToken)
        {
            var tokenArray = authorizationToken.Token.Split(new[] { TokenSeparator },
                StringSplitOptions.RemoveEmptyEntries);
            var accessToken = tokenArray[0];
            var tokenSecret = tokenArray[1];

            var reconnectResult = ReconnectRealm(ConsumerKey, ConsumerSecret, accessToken, tokenSecret);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(reconnectResult);

            string xpath = "ReconnectResponse";
            var root = xmlDoc.SelectSingleNode(xpath);
            var newToken = root.SelectSingleNode("//OAuthToken");
            var newTokenSecret = root.SelectSingleNode("//OAuthTokenSecret")?.Value;
            var authToken = new AuthorizationToken
            {
                Token = newTokenSecret
            };

            return Task.FromResult(authToken);
        }

        private string ReconnectRealm(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            HttpWebRequest httpWebRequest = WebRequest.Create("https://appcenter.intuit.com/api/v1/Connection/Reconnect") as HttpWebRequest;
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Authorization", GetDevDefinedOAuthHeader(httpWebRequest, consumerKey, consumerSecret, accessToken, accessTokenSecret));
            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            using (Stream data = httpWebResponse.GetResponseStream())
            {
                //return XML response
                return new StreamReader(data).ReadToEnd();
            }
        }

        private string GetDevDefinedOAuthHeader(HttpWebRequest webRequest, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            OAuthConsumerContext consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                SignatureMethod = SignatureMethod.HmacSha1,
                UseHeaderForOAuthParameters = true
            };

            consumerContext.UseHeaderForOAuthParameters = true;

            //URIs not used - we already have Oauth tokens
            OAuthSession oSession = new OAuthSession(consumerContext, "https://www.example.com",
                                    "https://www.example.com",
                                    "https://www.example.com");

            oSession.AccessToken = new TokenBase
            {
                Token = accessToken,
                ConsumerKey = consumerKey,
                TokenSecret = accessTokenSecret
            };

            IConsumerRequest consumerRequest = oSession.Request();
            consumerRequest = ConsumerRequestExtensions.ForMethod(consumerRequest, webRequest.Method);
            consumerRequest = ConsumerRequestExtensions.ForUri(consumerRequest, webRequest.RequestUri);
            consumerRequest = consumerRequest.SignWithToken();
            return consumerRequest.Context.GenerateOAuthParametersForHeader();
        }

    }
}