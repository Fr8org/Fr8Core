using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using Data.Entities;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Diagnostics;
using Intuit.Ipp.Security;
using terminalQuickBooks.Interfaces;
using Utilities.Configuration.Azure;


namespace terminalQuickBooks.Services
{
    public class Connectivity : IConnectivity
    {
        private static readonly ConcurrentDictionary<string, string> TokenSecrets =
            new ConcurrentDictionary<string, string>();

        private const string TokenSeperator = ";;;;;;;";

        private static readonly string AccessToken =
            CloudConfigurationManager.GetSetting("QuickBooksRequestTokenUrl").ToString(CultureInfo.InvariantCulture);

        private const string AccessTokenSecret = "";

        private static readonly string ConsumerKey =
            CloudConfigurationManager.GetSetting("QuickBooksConsumerKey").ToString(CultureInfo.InvariantCulture);

        private static readonly string ConsumerSecret =
            CloudConfigurationManager.GetSetting("QuickBooksConsumerSecret").ToString(CultureInfo.InvariantCulture);

        private static readonly string AppToken =
            CloudConfigurationManager.GetSetting("QuickBooksAppToken").ToString(CultureInfo.InvariantCulture);

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
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                SignatureMethod = SignatureMethod.HmacSha1
            };
            return new OAuthSession(consumerContext,
                AccessToken,
                CloudConfigurationManager.GetSetting("QuickBooksOAuthAuthorizeUrl")
                    .ToString(CultureInfo.InvariantCulture),
                CloudConfigurationManager.GetSetting("QuickBooksOAuthAccessUrl").ToString(CultureInfo.InvariantCulture));
        }

        public async Task<string> GetOAuthToken(string oauthToken, string oauthVerifier, string realmId)
        {
            var oauthSession = CreateSession();
            string tokenSecret;
            TokenSecrets.TryRemove(oauthToken, out tokenSecret);

            IToken reqToken = new RequestToken
            {
                Token = oauthToken,
                TokenSecret = tokenSecret,
                ConsumerKey =
                    CloudConfigurationManager.GetSetting("QuickBooksConsumerKey").ToString(CultureInfo.InvariantCulture)
            };
            var accToken = oauthSession.ExchangeRequestTokenForAccessToken(reqToken, oauthVerifier);

            return accToken.Token + TokenSeperator + accToken.TokenSecret + TokenSeperator + realmId;
        }

        public ServiceContext CreateServiceContext(string accessToken)
        {
            var tokens = accessToken.Split(new[] {TokenSeperator}, StringSplitOptions.None);
            var accToken = tokens[0];
            var accTokenSecret = tokens[1];
            var companyID = tokens[2];
            var oauthValidator = new OAuthRequestValidator(accToken, accTokenSecret, ConsumerKey, ConsumerSecret);
            return new ServiceContext(AppToken, companyID, IntuitServicesType.QBO, oauthValidator);
        }
        public DataService GetDataService(AuthorizationTokenDO authTokenDO)
        {
            var curServiceContext = CreateServiceContext(authTokenDO.Token);
            //Modify required settings for the Service Context
            curServiceContext.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";
            curServiceContext.IppConfiguration.MinorVersion.Qbo = "4";
            curServiceContext.IppConfiguration.Logger.RequestLog.EnableRequestResponseLogging = true;
            curServiceContext.IppConfiguration.Logger.CustomLogger = new TraceLogger();
            curServiceContext.IppConfiguration.Message.Response.SerializationFormat = SerializationFormat.Json;

            var curDataService = new DataService(curServiceContext);
            curServiceContext.UseDataServices();
            return curDataService;
        }
    }
}