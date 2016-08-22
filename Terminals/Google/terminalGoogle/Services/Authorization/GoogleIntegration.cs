using System;
using System.Net;
using System.Threading.Tasks;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Google.GData.Client;
using Newtonsoft.Json.Linq;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Services.Authorization
{
    public class GoogleIntegration : IGoogleIntegration
    {
        private readonly IRestfulServiceClient _client;

        public GoogleIntegration(IRestfulServiceClient serviceClient)
        {
            _client = serviceClient;
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

        public GoogleAuthDTO RefreshToken(GoogleAuthDTO googleAuthDTO)
        {
            var parameters = CreateOAuth2Parameters(
                accessToken: googleAuthDTO.AccessToken,
                refreshToken: googleAuthDTO.RefreshToken);
            OAuthUtil.RefreshAccessToken(parameters);
            googleAuthDTO.AccessToken = parameters.AccessToken;
            googleAuthDTO.Expires = parameters.TokenExpiry;
            return googleAuthDTO;
        }

        public async Task<string> GetExternalUserId(GoogleAuthDTO googleAuthDTO)
        {
            var url = CloudConfigurationManager.GetSetting("GoogleUserProfileUrl");
            url = url.Replace("%TOKEN%", googleAuthDTO.AccessToken);

            var jsonObj = await _client.GetAsync<JObject>(new Uri(url));
            return jsonObj.Value<string>("email");
        }

        /// <summary>
        /// Checks google token validity
        /// </summary>
        /// <param name="googleAuthDTO"></param>
        /// <returns></returns>
        public async Task<bool> IsTokenInfoValid(GoogleAuthDTO googleAuthDTO)
        {
            try
            {
                // Checks token for expiration local
                if (googleAuthDTO.Expires - DateTime.Now < TimeSpan.FromMinutes(5)
                    && string.IsNullOrEmpty(googleAuthDTO.RefreshToken))
                {
                    var message = "Google access token is expired. Token refresh will be executed";
                    //EventManager.TokenValidationFailed(JsonConvert.SerializeObject(googleAuthDTO), message);
                    Logger.GetLogger().Error(message);
                    return false;
                }

                // To validate token, we just need to make a get request to the GoogleTokenInfo url. 
                // We don't need result of this response, because if token is valid, there are no useful info for us;
                // if token is invalid, request fails with error code 400 and we catches this error below.
                var url = CloudConfigurationManager.GetSetting("GoogleTokenInfo");
                url = url.Replace("%TOKEN%", googleAuthDTO.AccessToken);
                await _client.GetAsync(new Uri(url));
                return true;
            }
            catch (Exception exception)
            {
                if (exception is RestfulServiceException || exception is WebException)
                {
                    var message = "Google token validation fails with error: " + exception.Message;
                    //EventManager.TokenValidationFailed(JsonConvert.SerializeObject(googleAuthDTO), message);
                    Logger.GetLogger().Error(message);
                    return false;
                }
                throw;
            }
        }
    }
}