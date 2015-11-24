using Google.Apis.Auth.OAuth2;
using Google.GData.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Services
{
    public class GoogleIntegration : IGoogleIntegration
    {
        public OAuth2Parameters CreateOAuth2Parameters(
           string accessCode = null,
           string accessToken = null,
           string refreshToken = null,
           string state = null)
        {
            return new OAuth2Parameters
            {
                ClientId = CloudConfigurationManager.GetSetting("GoogleClientId"),
                ClientSecret = CloudConfigurationManager.GetSetting("GoogleClientSecret"),
                Scope = CloudConfigurationManager.GetSetting("GoogleScope"),
                RedirectUri = CloudConfigurationManager.GetSetting("GoogleRedirectUri"),
                AccessCode = accessCode,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                State = state,
                AccessType = "offline",
                ApprovalPrompt = "force"
            };
        }

        public GOAuth2RequestFactory CreateRequestFactory(GoogleAuthDTO authDTO)
        {
            var parameters = CreateOAuth2Parameters(accessToken: authDTO.AccessToken, refreshToken: authDTO.RefreshToken);
            // Initialize the variables needed to make the request
            return new GOAuth2RequestFactory(null, "fr8", parameters);
        }

        public string CreateOAuth2AuthorizationUrl(string state = null)
        {
            var parameters = CreateOAuth2Parameters(state: state);
            return OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
        }

        public GoogleAuthDTO GetToken(string code)
        {
            var parameters = CreateOAuth2Parameters(accessCode: code);
            OAuthUtil.GetAccessToken(parameters);
            return new GoogleAuthDTO()
            {
                AccessToken = parameters.AccessToken,
                Expires = parameters.TokenExpiry,
                RefreshToken = parameters.RefreshToken
            };
        }

        public async Task<string> GetExternalUserId(GoogleAuthDTO authDTO)
        {
            var url = CloudConfigurationManager.GetSetting("GoogleUserProfileUrl");
            url = url.Replace("%TOKEN%", authDTO.AccessToken);

            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(url))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

                return jsonObj.Value<string>("email");
            }
        }
    }
}