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
        private static readonly ConcurrentDictionary<string, string> TokenSecrets = new ConcurrentDictionary<string, string>();
        private const string TokenSeperator = ";;;;;;;";
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

        public async Task<string> GetOAuthToken(string oauthToken, string oauthVerifier, string realmId)
        {
            var oauthSession = CreateSession();
            string tokenSecret;
            TokenSecrets.TryRemove(oauthToken, out tokenSecret);

            IToken reqToken = new RequestToken
            {
                Token = oauthToken,
                TokenSecret = tokenSecret,
                ConsumerKey = CloudConfigurationManager.GetSetting("QuickBooksConsumerKey").ToString(CultureInfo.InvariantCulture)
            };
            var accToken = oauthSession.ExchangeRequestTokenForAccessToken(reqToken, oauthVerifier);
            
            return accToken.Token+TokenSeperator+accToken.TokenSecret+TokenSeperator+realmId;
        }

        private ServiceContext CreateServiceContext(string accessToken)
        {
            var tokens = accessToken.Split(new[] { TokenSeperator }, StringSplitOptions.None);
            var accToken = tokens[0];
            var accTokenSecret = tokens[1];
            var companyID = tokens[2];
            var oauthValidator = new OAuthRequestValidator(accToken, accTokenSecret, CloudConfigurationManager.GetSetting("QuickBooksConsumerKey").ToString(CultureInfo.InvariantCulture), CloudConfigurationManager.GetSetting("QuickBooksConsumerSecret").ToString(CultureInfo.InvariantCulture));
            return new ServiceContext(CloudConfigurationManager.GetSetting("QuickBooksAppToken").ToString(CultureInfo.InvariantCulture), companyID, IntuitServicesType.QBO, oauthValidator);
        }

        public void CreateJournalEntry()
        {
            //move this to constructor of a seperate class
            var sc = CreateServiceContext("");
            var creditLine = new Line();
            creditLine.Description = "nov portion of rider insurance";
            creditLine.Amount = new Decimal(100.00);
            creditLine.AmountSpecified = true;
            creditLine.DetailType = LineDetailTypeEnum.JournalEntryLineDetail;
            creditLine.DetailTypeSpecified = true;
            var journalEntryLineDetailCredit = new JournalEntryLineDetail
            {
                PostingType = PostingTypeEnum.Credit,
                PostingTypeSpecified = true,
                AccountRef = new ReferenceType() {name = "Accumulated Depreciation", Value = "36"}
            };

            

            var je = new Intuit.Ipp.Data.JournalEntry();
            
            
        }

    }
}