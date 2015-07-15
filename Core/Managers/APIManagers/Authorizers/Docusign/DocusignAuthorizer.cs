using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Data.Infrastructure;
using Utilities;

namespace Core.Managers.APIManagers.Authorizers.Docusign
{
    class DocusignAuthorizer : IOAuthAuthorizer
    {
        private readonly Uri _docusignLoginFormUri;

        public DocusignAuthorizer(Uri docusignLoginFormUri)
        {
            _docusignLoginFormUri = docusignLoginFormUri;
        }

        private DocusignAuthFlow CreateFlow(string userId)
        {
            return new DocusignAuthFlow(userId, _docusignLoginFormUri);
        }

        public async Task<IOAuthAuthorizationResult> AuthorizeAsync(string userId, string email, string callbackUrl, string currentUrl,
            CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var result = await flow.AuthorizeAsync(callbackUrl);
            return result;
        }

        public Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    class DocusignAuthFlow
    {
        private readonly string _userId;
        private readonly Uri _docusignLoginFormUri;
        private readonly AuthData _authDataService;
        private readonly JSONDataStore _dataStore;

        public string Server { get; set; }
        public string ApiVersion { get; set; }

        public DocusignAuthFlow(string userId, Uri docusignLoginFormUri)
        {
            _userId = userId;
            _docusignLoginFormUri = docusignLoginFormUri;
            _authDataService = new AuthData();
            _dataStore = new JSONDataStore(GetUserDocusignAuthData, SetUserDocusignAuthData);
        }

        private void SetUserDocusignAuthData(string authData)
        {
            _authDataService.SetUserAuthData(_userId, "Docusign", authData);
        }

        private string GetUserDocusignAuthData()
        {
            return _authDataService.GetUserAuthData(_userId, "Docusign");
        }

        private async Task<bool> LoginAsync(string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                var response = await httpClient.GetAsync(
                            string.Format("https://{0}/restapi/{1}/login_information", Server, ApiVersion),
                            HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<IOAuthAuthorizationResult> AuthorizeAsync(string callbackUrl)
        {
            var accessToken = await _dataStore.GetAsync<string>("accessToken");
            if (!string.IsNullOrEmpty(accessToken))
            {
                var loginSucceeded = await LoginAsync(accessToken);
                if (loginSucceeded)
                    return new OAuthAuthorizationResult(isAuthorized: true);
            }
            return new OAuthAuthorizationResult(
                isAuthorized: false,
                redirectUri: _docusignLoginFormUri.AddOrUpdateQueryParams(new { callbackUrl }).ToString());
        }

        public async Task ObtainTokenAsync(string userName, string password)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(
                            string.Format("https://{0}/restapi/{1}/login_information", Server, ApiVersion),
                            HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
        }
    }
}
