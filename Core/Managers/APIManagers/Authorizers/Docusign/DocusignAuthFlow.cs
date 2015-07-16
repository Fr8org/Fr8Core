using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.Services;
using Data.Infrastructure;
using Newtonsoft.Json;
using Utilities;

namespace Core.Managers.APIManagers.Authorizers.Docusign
{
    class DocusignAuthFlow
    {
        private const string AccessTokenStoreKey = "accessToken";
        private readonly string _userId;
        private readonly Uri _docusignLoginFormUri;
        private readonly AuthData _authDataService;
        private readonly JSONDataStore _dataStore;

        public string Server { get; set; }
        public string ApiVersion { get; set; }
        public string IntegratorKey { get; set; }

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

        public async Task<IOAuthAuthorizationResult> AuthorizeAsync(string callbackUrl, string returnUrl)
        {
            var accessToken = await GetTokenAsync();
            if (!string.IsNullOrEmpty(accessToken))
            {
                var loginSucceeded = await LoginAsync(accessToken);
                if (loginSucceeded)
                    return new OAuthAuthorizationResult(isAuthorized: true);
            }
            return new OAuthAuthorizationResult(
                isAuthorized: false,
                redirectUri: new Uri(callbackUrl).AddOrUpdateQueryParams(new { returnUrl }).ToString());
        }

        public async Task ObtainTokenAsync(string userName, string password)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(
                    string.Format("https://{0}/restapi/{1}/oauth2/token", Server, ApiVersion),
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"), 
                        new KeyValuePair<string, string>("client_id", IntegratorKey), 
                        new KeyValuePair<string, string>("username", userName), 
                        new KeyValuePair<string, string>("password", password), 
                        new KeyValuePair<string, string>("scope", "api"), 
                    }));
                try
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException ex)
                    {
                        throw new OAuthException("Failed to obtain an access token. Credentials must have been incorrect.", ex);
                    }
                    var responseAsString = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeAnonymousType(responseAsString, new { access_token = "" });
                    await _dataStore.StoreAsync(AccessTokenStoreKey, responseObject.access_token);
                }
                finally
                {
                    response.Dispose();
                }
            }
        }

        public async Task RevokeTokenAsync()
        {
            var accessToken = await GetTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
                return;
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(
                    string.Format("https://{0}/restapi/{1}/oauth2/revoke", Server, ApiVersion),
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("token", accessToken), 
                    }));
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    throw new OAuthException("Failed to revoke an access token.", ex);
                }
                finally
                {
                    response.Dispose();
                }
                await _dataStore.DeleteAsync<string>(AccessTokenStoreKey);
            }
        }

        public async Task<string> GetTokenAsync()
        {
            return await _dataStore.GetAsync<string>(AccessTokenStoreKey);
        }
    }
}