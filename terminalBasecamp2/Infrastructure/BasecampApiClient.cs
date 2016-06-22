using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BasecampApiClient : IBasecampApiClient
    {
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly string _authorizationUrl;
        private readonly string _initialUrl;
        private readonly string _redirectUrl;
        private readonly string _tokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _contactEmail;

        private const string Basecamp2Product = "bcx";

        public BasecampApiClient(IRestfulServiceClient restfulServiceClient)
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
            _contactEmail = CloudConfigurationManager.GetSetting("ContactEmail");
        }

        public ExternalAuthUrlDTO GetExternalAuthUrl()
        {
            var externalStateToken = Guid.NewGuid().ToString();
            var url = $"{_initialUrl}?type=web_server&client_id={_clientId}&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}&state={externalStateToken}";
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
                var response = await _restfulServiceClient.PostAsync(new Uri(url)).ConfigureAwait(false);
                var oAuthResponse = JsonConvert.DeserializeObject<OAuthResponse>(response);
                var basecampAuthorizationDTO = new BasecampAuthorizationToken
                {
                    AccessToken = oAuthResponse.AccessToken,
                    RefreshToken = oAuthResponse.RefreshToken,
                };
                //Retrieving info about username and project
                var userInfo = await GetCurrentUserInfo(basecampAuthorizationDTO).ConfigureAwait(false);
                if (userInfo.Accounts.TrueForAll(x => x.Product != Basecamp2Product))
                {
                    Logger.LogError($"Authorized user doesn't have access to Basecamp2. Fr8 User Id - {externalState.Fr8UserId}");
                    return new AuthorizationTokenDTO { Error = "We couldn't authorize you as your account doesn't have access to Basecamp2 product" };
                }
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

        public async Task<Authorization> GetCurrentUserInfo(AuthorizationToken auth)
        {
            return await GetCurrentUserInfo(GetAuthorization(auth)).ConfigureAwait(false);
        }

        private async Task<Authorization> GetCurrentUserInfo(BasecampAuthorizationToken auth)
        {
            return await ApiGetAsync<Authorization>(_authorizationUrl, auth).ConfigureAwait(false);
        }

        public async Task<List<Account>> GetAccounts(AuthorizationToken auth)
        {
            return (await GetCurrentUserInfo(auth).ConfigureAwait(false))
                .Accounts
                .Where(x => x.Product == Basecamp2Product)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<List<Project>> GetProjects(string accountUrl, AuthorizationToken auth)
        {
            var result = await ApiGetAsync<List<Project>>($"{accountUrl}/projects.json", GetAuthorization(auth));
            result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            return result;
        }

        public async Task CreateMessage(string accountUrl, string projectId, string subject, string content, AuthorizationToken auth)
        {
            await ApiPostAsync($"{accountUrl}/projects/{projectId}/messages.json", new Message { Subject = subject, Content = content }, GetAuthorization(auth));
        }

        private async Task ApiPostAsync<TContent>(string apiUrl, TContent content, BasecampAuthorizationToken auth)
        {
            await _restfulServiceClient.PostAsync<TContent, object>(new Uri(apiUrl), content, headers: GetHeaders(auth)).ConfigureAwait(false);
        }

        private async Task<TResponse> ApiPostAsync<TResponse>(string apiUrl, BasecampAuthorizationToken auth)
        {
            return await _restfulServiceClient.PostAsync<TResponse>(new Uri(apiUrl), headers: GetHeaders(auth)).ConfigureAwait(false);
        }

        private async Task<TResponse> ApiGetAsync<TResponse>(string apiUrl, BasecampAuthorizationToken auth)
        {
            return await _restfulServiceClient.GetAsync<TResponse>(new Uri(apiUrl), headers: GetHeaders(auth)).ConfigureAwait(false);
        }

        private Dictionary<string, string> GetHeaders(BasecampAuthorizationToken auth)
        {
            return new Dictionary<string, string>
                   {
                       { "Authorization", $"Bearer {auth.AccessToken}" },
                       { "User-Agent", $"Fr8.co ({_contactEmail})" }
                   };
        }

        private BasecampAuthorizationToken GetAuthorization(AuthorizationToken token)
        {
            return GetAuthorization(token.Token);
        }

        private BasecampAuthorizationToken GetAuthorization(string token)
        {
            return JsonConvert.DeserializeObject<BasecampAuthorizationToken>(token);
        }
    }
}