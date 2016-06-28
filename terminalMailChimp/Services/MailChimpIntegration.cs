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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalMailChimp.Interfaces;

namespace terminalMailChimp.Services
{
    public class MailChimpIntegration : IMailChimpIntegration
    {
        private readonly IRestfulServiceClient _restfulServiceClient;

        private string MailChimpClientId => CloudConfigurationManager.GetSetting("MailChimpClientId");

        private string MailChimpClientSecret => CloudConfigurationManager.GetSetting("MailChimpClientSecret");

        private string HubOAuthRedirectUri => CloudConfigurationManager.GetSetting("HubOAuthRedirectUri");

        private string MailChimpAuthorizeUri => CloudConfigurationManager.GetSetting("MailChimpAuthorizeUri");

        private string MailChimpAccessTokenUri => CloudConfigurationManager.GetSetting("MailChimpAccessTokenUri");

        private string MailChimpOAuthAccessUrlContent => CloudConfigurationManager.GetSetting("MailChimpOAuthAccessUrlContent");

        public MailChimpIntegration(IRestfulServiceClient restfulServiceClient)
        {
            _restfulServiceClient = restfulServiceClient;
        }

        public ExternalAuthUrlDTO GenerateOAuthInitialUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();

            var url = MailChimpAuthorizeUri.Replace("%STATE%", externalStateToken)
                .Replace("%CLIENTID%", MailChimpClientId)
                .Replace("%HUBOAUTHREDIRECTURI%", HubOAuthRedirectUri);
            
            return new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };
        }

        public async Task<string> GetToken(string code)
        {
            var payload = MailChimpOAuthAccessUrlContent.Replace("%CLIENTID%", MailChimpClientId)
                .Replace("%CLIENTSECRET%", MailChimpClientSecret)
                .Replace("%HUBOAUTHREDIRECTURI%", HubOAuthRedirectUri)
                .Replace("%CODE%", code); ;
            
            var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("User-Agent", "oauth2-draft-v10");

            var request = new HttpRequestMessage(HttpMethod.Post, MailChimpAccessTokenUri)
            {
                Content = new StringContent(payload, Encoding.UTF8,
                    "application/x-www-form-urlencoded")
            };

            var defaultRequestHeaders = new Dictionary<string, string>()
            {

            };

            _restfulServiceClient.PostAsync(new Uri(MailChimpAccessTokenUri), new StringContent(payload, Encoding.UTF8,
                    "application/x-www-form-urlencoded"),null, new Dictionary<string, string>() {  )

            var result = await httpClient.SendAsync(request);

            var response = await result.Content.ReadAsStringAsync();
            var jsonObj = JsonConvert.DeserializeObject<JObject>(response);

            return jsonObj.Value<string>("access_token");
        }

        public Task<string> GetExternalUserId(object oauthToken)
        {
            throw new NotImplementedException();
        }
    }
}