using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using terminalBasecamp.Data;

namespace terminalBasecamp.Infrastructure
{
    public class BasecampApi : IBasecampApi
    {
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly string _authorizationUrl;
        private readonly string _initialUrl;
        private readonly string _redirectUrl;
        private readonly string _tokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public BasecampApi(IRestfulServiceClient restfulServiceClient)
        {
            if (restfulServiceClient == null)
            {
                throw new ArgumentNullException(nameof(restfulServiceClient));
            }
            _restfulServiceClient = restfulServiceClient;
            _initialUrl = CloudConfigurationManager.GetSetting("AppInitialUrl");
            _redirectUrl = CloudConfigurationManager.GetSetting("AppRedirectUrl");
            _authorizationUrl = CloudConfigurationManager.GetSetting("AppAuthorizationUrl");
            _tokenUrl = CloudConfigurationManager.GetSetting("AppTokenUrl");
            _clientId = CloudConfigurationManager.GetSetting("AppClientId");
            _clientSecret = CloudConfigurationManager.GetSetting("AppClientSecret");
        }

        public ExternalAuthUrlDTO GetExternalAuthUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = $"{_initialUrl}?type=web_server&client_id={_clientId}&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}&state={HttpUtility.UrlEncode(externalStateToken)}";
            return new ExternalAuthUrlDTO
            {
                ExternalStateToken = externalStateToken,
                Url = url
            };
        }

        public async Task<AuthorizationTokenDTO> AuthenticateAsync(ExternalAuthenticationDTO externalState)
        {
            try
            {
                var query = HttpUtility.ParseQueryString(externalState.RequestQueryString);
                var code = query["code"];
                var state = query["state"];
                var url = $"{_tokenUrl}?type=web_server&client_id={_clientId}&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}&client_secret={_clientSecret}&code={code}";
                var response = await _restfulServiceClient.PostAsync(new Uri(url));
                var oAuthResponse = JsonConvert.DeserializeObject<OAuthResponse>(response);
                var basecampAuthorizationDTO = new BasecampAuthorizationDTO
                {
                    AccessToken = oAuthResponse.AccessToken,
                    RefreshToken = oAuthResponse.RefreshToken,
                };
                //Retrieving info about username and project
                var userInfo = await GetCurrentUserInfo(basecampAuthorizationDTO);
                return new AuthorizationTokenDTO
                {
                    ExternalDomainId = string.Empty,
                    ExternalAccountId = userInfo.Identity.Id.ToString(),
                    ExternalAccountName = userInfo.Identity.DisplayName,
                    ExternalStateToken = state,
                    Token = JsonConvert.SerializeObject(basecampAuthorizationDTO)
                };
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to authorize with Basecamp. Fr8 User Id - {externalState.Fr8UserId}, Details - {ex}");
                return new AuthorizationTokenDTO { Error = "An error occured while trying to authorize you. Please try again later" };
            }
        }

        public async Task<Authorization> GetCurrentUserInfo(AuthorizationToken authorizationToken)
        {
            return await ApiGetAsync<Authorization>(_authorizationUrl, GetAuthorization(authorizationToken));
        }

        public async Task<List<Account>> GetAccounts(AuthorizationToken authorizationToken)
        {
            return (await GetCurrentUserInfo(authorizationToken)).Accounts;
        }

        private async Task<TResponse> ApiPostAsync<TResponse>(string apiUrl, BasecampAuthorizationDTO auth)
        {
            return await _restfulServiceClient.PostAsync<TResponse>(new Uri(apiUrl), headers: new Dictionary<string, string> { { "Authorization", $"Bearer {HttpUtility.UrlEncode(auth.AccessToken)}" } });
        }

        private async Task<TResponse> ApiGetAsync<TResponse>(string apiUrl, BasecampAuthorizationDTO auth)
        {
            return await _restfulServiceClient.GetAsync<TResponse>(new Uri(apiUrl), headers: new Dictionary<string, string> { { "Authorization", $"Bearer {HttpUtility.UrlEncode(auth.AccessToken)}" } });
        }

        private BasecampAuthorizationDTO GetAuthorization(AuthorizationToken token)
        {
            return GetAuthorization(token.Token);
        }

        private BasecampAuthorizationDTO GetAuthorization(string token)
        {
            return JsonConvert.DeserializeObject<BasecampAuthorizationDTO>(token);
        }
    }
}