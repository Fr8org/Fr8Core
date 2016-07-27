using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalBasecamp2.Data;
using Authorization = terminalBasecamp2.Data.Authorization;

namespace terminalBasecamp2.Infrastructure
{
    public class BasecampApiClient : OAuthApiIntegrationBase, IBasecampApiClient
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

        public BasecampApiClient(IRestfulServiceClient restfulServiceClient, IHubCommunicator hubCommunicator) : base(hubCommunicator)
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

        /// <summary>
        /// Gets URL that is used by Basecamp to aks user for authorization
        /// </summary>
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

        /// <summary>
        /// Performs OAuth authentication after receiving verification code from Basecamp. Additionaly performs a check of whether use has access to Basecamp2 projects
        /// </summary>
        public async Task<AuthorizationTokenDTO> AuthenticateAsync(ExternalAuthenticationDTO externalState)
        {
            try
            {
                var query = HttpUtility.ParseQueryString(externalState.RequestQueryString);
                var code = query["code"];
                var state = query["state"];
                var url = $"{_tokenUrl}?type=web_server&client_id={_clientId}&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}&client_secret={_clientSecret}&code={code}";
                var response = await _restfulServiceClient.PostAsync<OAuthResponse>(new Uri(url)).ConfigureAwait(false);
                var basecampAuthorizationDTO = new BasecampAuthorizationToken
                {
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                };
                //Retrieving info about username and project
                var userInfo = await GetCurrentUserInfo(basecampAuthorizationDTO).ConfigureAwait(false);
                if (userInfo.Accounts.TrueForAll(x => x.Product != Basecamp2Product))
                {
                    Logger.GetLogger().Error($"Authorized user doesn't have access to Basecamp2. Fr8 User Id - {externalState.Fr8UserId}");
                    return new AuthorizationTokenDTO { Error = "We couldn't authorize you as your account doesn't have access to Basecamp2 product" };
                }
                basecampAuthorizationDTO.ExpiresAt = userInfo.ExpiresAt;
                return new AuthorizationTokenDTO
                {
                    ExternalAccountId = userInfo.Identity.Id.ToString(),
                    ExternalAccountName = userInfo.Identity.DisplayName,
                    ExternalStateToken = state,
                    ExpiresAt = basecampAuthorizationDTO.ExpiresAt,
                    Token = JsonConvert.SerializeObject(basecampAuthorizationDTO)
                };
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error($"Failed to authorize with Basecamp. Fr8 User Id - {externalState.Fr8UserId}, Details - {ex}");
                return new AuthorizationTokenDTO { Error = "An error occured while trying to authorize you. Please try again later" };
            }
        }

        /// <summary>
        /// Gets basic information about authorized user along with accounts available to him
        /// </summary>
        public async Task<Authorization> GetCurrentUserInfo(AuthorizationToken auth)
        {
            return await ApiCall(x => ApiGetAsync<Authorization>(_authorizationUrl, GetAuthorization(x)), auth);
        }

        private async Task<Authorization> GetCurrentUserInfo(BasecampAuthorizationToken auth)
        {
            return await ApiGetAsync<Authorization>(_authorizationUrl, auth).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets accounts available to authorized user
        /// </summary>
        public async Task<List<Account>> GetAccounts(AuthorizationToken auth)
        {
            return (await GetCurrentUserInfo(auth).ConfigureAwait(false))
                .Accounts
                .Where(x => x.Product == Basecamp2Product)
                .OrderBy(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Get projects belong to specified account and available for authorized user
        /// </summary>
        public async Task<List<Project>> GetProjects(string accountUrl, AuthorizationToken auth)
        {
            var result = await ApiCall(x => ApiGetAsync<List<Project>>($"{accountUrl}/projects.json", GetAuthorization(x)), auth);
            result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            return result;
        }

        /// <summary>
        /// Post a new message into specified project of authorized user. Message with the same subject and content can't be posted to Basecamp2 during period of 5 minutes
        /// </summary>
        /// <returns>A message that has been posted</returns>
        public async Task<Message> CreateMessage(string accountUrl, string projectId, string subject, string content, AuthorizationToken auth)
        {
            try
            {
                return await ApiCall(x => ApiPostAsync<Message, Message>($"{accountUrl}/projects/{projectId}/messages.json", new Message { Subject = subject, Content = content }, GetAuthorization(x)), auth);
            }
            catch (RestfulServiceException ex)
            {
                if (ex.StatusCode == 422)
                {
                    var errorMessage = JToken.Parse(ex.ResponseMessage).Value<string>("error");
                    throw new ApplicationException(errorMessage, ex);
                }
                throw;
            }
        }

        private async Task<TResponse> ApiPostAsync<TContent, TResponse>(string apiUrl, TContent content, BasecampAuthorizationToken auth)
        {
            return await _restfulServiceClient.PostAsync<TContent, TResponse>(new Uri(apiUrl), content, headers: GetHeaders(auth)).ConfigureAwait(false);
        }

        private async Task<TResponse> ApiGetAsync<TResponse>(string apiUrl, BasecampAuthorizationToken auth)
        {
            return await _restfulServiceClient.GetAsync<TResponse>(new Uri(apiUrl), headers: GetHeaders(auth)).ConfigureAwait(false);
        }

        protected override async Task<AuthorizationToken> RefreshToken(AuthorizationToken auth)
        {
            var basecampAuth = GetAuthorization(auth);
            var url = $"{_tokenUrl}?type=refresh&client_id={_clientId}&redirect_uri={HttpUtility.UrlEncode(_redirectUrl)}&client_secret={_clientSecret}&refresh_token={basecampAuth.RefreshToken}";
            var now = DateTime.UtcNow;
            var response = await _restfulServiceClient.PostAsync<OAuthResponse>(new Uri(url)).ConfigureAwait(false);
            basecampAuth.AccessToken = response.AccessToken;
            basecampAuth.ExpiresAt = now.AddSeconds(response.ExpiresIn);
            return new AuthorizationToken
            {
                ExternalAccountId = auth.ExternalAccountId,
                ExternalAccountName = auth.ExternalAccountName,
                ExternalStateToken = auth.ExternalStateToken,
                ExpiresAt = basecampAuth.ExpiresAt,
                Id = auth.Id,
                TerminalID = auth.TerminalID,
                UserId = auth.UserId,
                Token = JsonConvert.SerializeObject(basecampAuth)
            };
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