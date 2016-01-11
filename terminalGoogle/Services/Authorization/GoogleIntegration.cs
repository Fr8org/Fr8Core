using Google.Apis.Auth.OAuth2;
using Google.GData.Client;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
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

        private readonly IRestfulServiceClient _client;

        public GoogleIntegration()
        {
            _client = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }

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

            var jsonObj = await _client.GetAsync<JObject>(new Uri(url));
            return jsonObj.Value<string>("email");
        }
    }
}