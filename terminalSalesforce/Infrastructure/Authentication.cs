using Data.Interfaces.DataTransferObjects;
using terminalSalesforce.Services;
using Salesforce.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Utilities.Configuration.Azure;

namespace terminalSalesforce.Infrastructure
{
    public class Authentication
    {        
        string salesforceConsumerKey = string.Empty;
        string salesforceAuthUrl = string.Empty;
        string SalesforceAuthCallbackURLDomain = string.Empty;
        string salesforceConsumerSecret = string.Empty;
        string tokenRequestEndpointUrl = string.Empty;

        public Authentication()
        {
            salesforceConsumerKey = CloudConfigurationManager.GetSetting("SalesforceConsumerKey");
            salesforceAuthUrl = CloudConfigurationManager.GetSetting("SalesforceAuthURL");
            SalesforceAuthCallbackURLDomain = CloudConfigurationManager.GetSetting("SalesforceAuthCallbackURLDomain");
            salesforceConsumerSecret = CloudConfigurationManager.GetSetting("SalesforceConsumerSecret");
            tokenRequestEndpointUrl = CloudConfigurationManager.GetSetting("tokenRequestEndpointUrl");
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

        private void ParseCodeAndState(string queryString, out string code, out string state)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ApplicationException("QueryString is empty.");
            }

            code = null;
            state = null;

            var tokens = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var nameValueTokens = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValueTokens.Length < 2)
                {
                    continue;
                }

                if (nameValueTokens[0] == "code")
                {
                    code = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "state")
                {
                    state = nameValueTokens[1];
                }
            }
        }

        public AuthTokenDTO Authenticate(
            ExternalAuthenticationDTO externalAuthDTO)
        {
            string code;
            string state;
            
            //Code will be access code returned by the Salesforce after the user authenticates app
            //State is be value passed by us to salesforce and it's returned by the salesforce after the authentication
            ParseCodeAndState(externalAuthDTO.RequestQueryString, out code, out state);         

            AuthenticationClient oauthToken = (AuthenticationClient)Task.Run(() => GetAuthToken(code)).Result;         
          
            return new AuthTokenDTO()
            {
                Token = oauthToken.AccessToken,
                ExternalAccountId = oauthToken.Id.Substring(oauthToken.Id.LastIndexOf("/")+1,oauthToken.Id.Length-(oauthToken.Id.LastIndexOf("/")+1)),
                ExternalStateToken =state,
                AdditionalAttributes = "refresh_token="+oauthToken.RefreshToken+";instance_url="+oauthToken.InstanceUrl+";api_version="+oauthToken.ApiVersion               
            };
        }


        public async Task<object> GetAuthToken(string code)
        {
            var auth = new AuthenticationClient();
            code = code.Replace("%3D", "=");
            string redirectUrl = SalesforceAuthCallbackURLDomain + "/ExternalAuth/AuthSuccess?dockyard_plugin=pluginSalesforce&version=1";
            await auth.WebServerAsync(salesforceConsumerKey, salesforceConsumerSecret, redirectUrl, code, tokenRequestEndpointUrl);
            return auth;
        }


        public string CreateAuthUrl(string exteranalStateValue)
        {
            string redirectUrl = SalesforceAuthCallbackURLDomain + "/ExternalAuth/AuthSuccess?dockyard_plugin=pluginSalesforce&version=1";
            string url = Common.FormatAuthUrl(
                salesforceAuthUrl, Salesforce.Common.Models.ResponseTypes.Code,
                salesforceConsumerKey,
                HttpUtility.UrlEncode(redirectUrl), Salesforce.Common.Models.DisplayTypes.Page, false, HttpUtility.UrlEncode(exteranalStateValue), "full%20refresh_token"
                );
            return url;
        }

        public async Task<ActionDTO> RefreshAccessToken(ActionDTO currentActionDTO)
        {
            var auth = new AuthenticationClient();
            string authAttributes = currentActionDTO.AuthToken.AdditionalAttributes;
            string refreshToken = authAttributes.Substring(authAttributes.IndexOf("refresh_token"), authAttributes.IndexOf("instance_url") - 1);
            refreshToken = refreshToken.Replace("refresh_token=", "");
            await auth.TokenRefreshAsync(salesforceConsumerKey, refreshToken);
            currentActionDTO.AuthToken.Token = auth.AccessToken;
            currentActionDTO.AuthToken.AdditionalAttributes = "refresh_token=" + auth.RefreshToken + ";instance_url=" + auth.InstanceUrl + ";api_version=" + auth.ApiVersion;            
            return currentActionDTO;
        }
    }
}