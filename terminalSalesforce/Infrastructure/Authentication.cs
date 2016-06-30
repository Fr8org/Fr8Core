using Newtonsoft.Json.Linq;
using Salesforce.Force;
using Salesforce.Common;
using System;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;

namespace terminalSalesforce.Infrastructure
{
    public class Authentication
    {
        string salesforceConsumerKey = string.Empty;
        string salesforceAuthUrl = string.Empty;
        string SalesforceAuthCallbackURLDomain = string.Empty;
        string salesforceConsumerSecret = string.Empty;
        string tokenRequestEndpointUrl = string.Empty;
        string salesforceTerminalName = string.Empty;
        string salesforceTerminalVersion = string.Empty;

        public Authentication()
        {
            salesforceConsumerKey = CloudConfigurationManager.GetSetting("SalesforceConsumerKey");
            salesforceAuthUrl = CloudConfigurationManager.GetSetting("SalesforceAuthURL");
            SalesforceAuthCallbackURLDomain = CloudConfigurationManager.GetSetting("SalesforceAuthCallbackURLDomain");
            salesforceConsumerSecret = CloudConfigurationManager.GetSetting("SalesforceConsumerSecret");
            tokenRequestEndpointUrl = CloudConfigurationManager.GetSetting("tokenRequestEndpointUrl");
            salesforceTerminalName = CloudConfigurationManager.GetSetting("salesforceTerminalNameQueryParam");
            salesforceTerminalVersion = CloudConfigurationManager.GetSetting("salesforceTerminalVersionQueryParam");
        }

        public ExternalAuthUrlDTO GetExternalAuthUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = CreateAuthUrl(externalStateToken);
            var externalAuthUrlDTO = new ExternalAuthUrlDTO()
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };
            return externalAuthUrlDTO;
        }

        public AuthorizationTokenDTO Authenticate(ExternalAuthenticationDTO externalAuthDTO)
        {
            var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
            //Code will be access code returned by the Salesforce after the user authenticates app
            string code = query["code"];
            //State is be value passed by us to salesforce and it's returned by the salesforce after the authentication
            string state = query["state"];

            AuthenticationClient authClient = (AuthenticationClient)Task.Run(() => GetAuthToken(code)).Result;

            string username = "";
            string externalAccountId = "";
            var curUserInfo =
                  Task.Run(
                      () =>
                          new ForceClient(authClient.InstanceUrl, authClient.AccessToken, authClient.ApiVersion)
                              .UserInfo<object>(authClient.Id)).Result;

            JToken propertyValue;

            var jCurUserInfo = (JObject)curUserInfo;
            if (jCurUserInfo.TryGetValue("display_name", out propertyValue))
            {
                username = propertyValue.Value<string>();
            }

            if (jCurUserInfo.TryGetValue("user_id", out propertyValue))
            {
                externalAccountId = propertyValue.Value<string>();
            }

            return new AuthorizationTokenDTO()
            {
                Token = JsonConvert.SerializeObject(new { AccessToken = authClient.AccessToken, RefreshToken = authClient.RefreshToken }),
                ExternalAccountId = externalAccountId,
                ExternalAccountName = username,
                ExternalStateToken = state,
                AdditionalAttributes = $"instance_url={authClient.InstanceUrl};api_version={authClient.ApiVersion}"
            };
        }


        public async Task<object> GetAuthToken(string code)
        {
            var auth = new AuthenticationClient();
            code = code.Replace("%3D", "=");
            string redirectUrl = SalesforceAuthCallbackURLDomain + salesforceTerminalName + "&" + salesforceTerminalVersion;
            await auth.WebServerAsync(salesforceConsumerKey, salesforceConsumerSecret, redirectUrl, code, tokenRequestEndpointUrl);
            return auth;
        }


        public string CreateAuthUrl(string exteranalStateValue)
        {
            string redirectUrl = SalesforceAuthCallbackURLDomain + salesforceTerminalName + "&" + salesforceTerminalVersion;
            string url = Common.FormatAuthUrl(
                salesforceAuthUrl, 
                Salesforce.Common.Models.ResponseTypes.Code,
                salesforceConsumerKey,
                HttpUtility.UrlEncode(redirectUrl), 
                Salesforce.Common.Models.DisplayTypes.Page, 
                false, 
                HttpUtility.UrlEncode(exteranalStateValue),
                "full%20refresh_token"
                );
            return url;
        }

        public async Task<AuthorizationToken> RefreshAccessToken(AuthorizationToken curAuthToken)
        {
            var authClient = new AuthenticationClient();
            var refreshToken = (string)JsonConvert.DeserializeObject<dynamic>(curAuthToken.Token).RefreshToken;

            //In Test scenario, the refresh token will be empty as we use Salesforce's UserName & Password login method. 
            //In that case, there will no refresh token be available. Return the input auth token.
            if(string.IsNullOrEmpty(refreshToken))
            {
                return curAuthToken;
            }

            await authClient.TokenRefreshAsync(salesforceConsumerKey, refreshToken);            
            curAuthToken.Token = JsonConvert.SerializeObject(new { AccessToken = authClient.AccessToken, RefreshToken = refreshToken });
            curAuthToken.AdditionalAttributes = $"instance_url={authClient.InstanceUrl};api_version={authClient.ApiVersion}";
            return curAuthToken;
        }
    }
}