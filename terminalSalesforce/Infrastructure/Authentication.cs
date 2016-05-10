using Newtonsoft.Json.Linq;
using Salesforce.Force;
using Salesforce.Common;
using System;
using System.Threading.Tasks;
using System.Web;
using Utilities.Configuration.Azure;
using Data.Entities;
using Fr8Data.DataTransferObjects;

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

        public AuthorizationTokenDTO Authenticate(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
            //Code will be access code returned by the Salesforce after the user authenticates app
            string code = query["code"];
            //State is be value passed by us to salesforce and it's returned by the salesforce after the authentication
            string state = query["state"];

            AuthenticationClient authClient = (AuthenticationClient)Task.Run(() => GetAuthToken(code)).Result;

            //By Default, Salesforce returns the User ID of the currently logged in user which is not friendly one.
            var friendlyUserName = GetFriendlyUserName(authClient);

            return new AuthorizationTokenDTO()
            {
                Token = authClient.AccessToken,
                ExternalAccountId = friendlyUserName,
                ExternalStateToken = state,
                AdditionalAttributes = $"refresh_token={authClient.RefreshToken};" +
                                       $"instance_url={authClient.InstanceUrl};" +
                                       $"api_version={authClient.ApiVersion}"
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

        public async Task<AuthorizationTokenDO> RefreshAccessToken(AuthorizationTokenDO curAuthTokenDO)
        {
            var authClient = new AuthenticationClient();
            string authAttributes = curAuthTokenDO.AdditionalAttributes;
            string refreshToken = authAttributes.Substring(authAttributes.IndexOf("refresh_token"), authAttributes.IndexOf("instance_url") - 1);
            refreshToken = refreshToken.Replace("refresh_token=", "");

            //In Test scenario, the refresh token will be empty as we use Salesforce's UserName & Password login method. 
            //In that case, there will no refresh token be available. Return the input auth token.
            if(string.IsNullOrEmpty(refreshToken))
            {
                return curAuthTokenDO;
            }

            await authClient.TokenRefreshAsync(salesforceConsumerKey, refreshToken);
            curAuthTokenDO.Token = authClient.AccessToken;
            curAuthTokenDO.AdditionalAttributes = $"refresh_token={authClient.RefreshToken};" +
                                                  $"instance_url={authClient.InstanceUrl};" +
                                                  $"api_version={authClient.ApiVersion}";
            return curAuthTokenDO;
        }

        /// <summary>
        /// Gets Salesforce Friendly Name for the Auth Token User ID.
        /// </summary>
        private string GetFriendlyUserName(AuthenticationClient oauthToken)
        {
            string userName = string.Empty;

            var curUserInfo =
                    Task.Run(
                        () =>
                            new ForceClient(oauthToken.InstanceUrl, oauthToken.AccessToken, oauthToken.ApiVersion)
                                .UserInfo<object>(oauthToken.Id)).Result;

            JToken propertyValue;

            var jCurUserInfo = (JObject) curUserInfo; 
            if (jCurUserInfo.TryGetValue("display_name", out propertyValue))
            {
                userName = propertyValue.Value<string>();
            }

            if (jCurUserInfo.TryGetValue("user_id", out propertyValue))
            {
                userName += string.Format(" [{0}]", propertyValue.Value<string>());
            }

            return userName;
        }
    }
}