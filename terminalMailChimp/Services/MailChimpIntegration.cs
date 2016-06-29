using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalMailChimp.Interfaces;
using terminalMailChimp.Models;

namespace terminalMailChimp.Services
{
    public class MailChimpIntegration : IMailChimpIntegration
    {
        #region Properties

        private readonly IRestfulServiceClient _restfulServiceClient;

        private static string MailChimpClientId => CloudConfigurationManager.GetSetting("MailChimpClientId");

        private static string MailChimpClientSecret => CloudConfigurationManager.GetSetting("MailChimpClientSecret");

        private static string HubOAuthRedirectUri => CloudConfigurationManager.GetSetting("HubOAuthRedirectUri");

        private static string MailChimpAuthorizeUri => CloudConfigurationManager.GetSetting("MailChimpAuthorizeUri");

        private static string MailChimpAccessTokenUri => CloudConfigurationManager.GetSetting("MailChimpAccessTokenUri");

        private static string MailChimpOAuthAccessUrlContent => CloudConfigurationManager.GetSetting("MailChimpOAuthAccessUrlContent");

        private static string MailChimpDataCenterApiEndpoint => CloudConfigurationManager.GetSetting("MailChimpDataCenterApiEndpoint");

        #endregion

        public MailChimpIntegration(IRestfulServiceClient restfulServiceClient)
        {
            _restfulServiceClient = restfulServiceClient;
        }

        public ExternalAuthUrlDTO GenerateOAuthInitialUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();

            var url = MailChimpAuthorizeUri.Replace("%STATE%", externalStateToken)
                .Replace("%CLIENTID%", MailChimpClientId)
                .Replace("%HUBOAUTHREDIRECTURI%", HttpUtility.UrlEncode(HubOAuthRedirectUri));
            
            return new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };
        }

        public async Task<List<MailChimpList>> GetLists(AuthorizationToken authorizationToken)
        {
            var result = new List<MailChimpList>();


            return result;
        }

        public async Task<AuthorizationTokenDTO> GetAuthToken(string code, string state)
        {
            // get access token from Mailchimp
            var payload = MailChimpOAuthAccessUrlContent.Replace("%CLIENTID%", MailChimpClientId)
                .Replace("%CLIENTSECRET%", MailChimpClientSecret)
                .Replace("%HUBOAUTHREDIRECTURI%", HttpUtility.UrlEncode(HubOAuthRedirectUri))
                .Replace("%CODE%", code); 

            var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "oauth2-draft-v10");

            var request = new HttpRequestMessage(HttpMethod.Post, MailChimpAccessTokenUri)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/x-www-form-urlencoded")
            };

            var result = await httpClient.SendAsync(request);
            var response = await result.Content.ReadAsStringAsync();
            var jsonObj = JsonConvert.DeserializeObject<JObject>(response);

            var accessToken = jsonObj.Value<string>("access_token");

            //get account details   
            var accountDetailsResponse = await _restfulServiceClient.GetAsync(new Uri(MailChimpDataCenterApiEndpoint), null,
                new Dictionary<string, string>() {{"Authorization", $"apikey {accessToken}"}});

            var jsonAccountDetails = JsonConvert.DeserializeObject<JObject>(accountDetailsResponse);

            return new AuthorizationTokenDTO()
            {
                Token = accessToken,
                ExternalStateToken = state,
                ExternalAccountId = jsonAccountDetails.Value<string>("account_id"),
                ExternalAccountName = jsonAccountDetails.Value<string>("account_name")
            };
        }

        public Task<string> GetExternalUserId(object oauthToken)
        {
            throw new NotImplementedException();
        }
    }
}