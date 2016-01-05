using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Hub.Interfaces;
using Hub.Services;
using Utilities;

namespace Hub.Managers.APIManagers.Authorizers.Docusign
{
    class DocuSignAuthFlow
    {
        private readonly string _userId;
        private readonly Authorization _authorizationToken;

        public string Endpoint { get; set; }
        public string IntegratorKey { get; set; }

        public DocuSignAuthFlow(string userId)
        {
            _userId = userId;
            _authorizationToken = new Authorization();
        }

        private HttpClient CreateClient(string accessToken = null)
        {
            var httpClient = new HttpClient() {BaseAddress = new Uri(Endpoint)};
            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            }
            return httpClient;
        }

        private async Task<bool> LoginAsync(string accessToken)
        {
            using (var httpClient = CreateClient(accessToken))
            {
                var response = await httpClient.GetAsync(
                    "login_information",
                    HttpCompletionOption.ResponseHeadersRead);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<IOAuthAuthorizationResult> AuthorizeAsync(string callbackUrl, string currentUrl)
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
                redirectUri: new Uri(callbackUrl).ToString());
        }

        public async Task ObtainTokenAsync(string userName, string password)
        {
            using (var httpClient = CreateClient())
            {
                var response = await httpClient.PostAsync(
                    "oauth2/token",
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
                    _authorizationToken.AddOrUpdateToken(_userId, responseObject.access_token);
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
            using (var httpClient = CreateClient())
            {
                var response = await httpClient.PostAsync(
                    "oauth2/revoke",
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
                _authorizationToken.RemoveToken(_userId);
            }
        }

        public Task<string> GetTokenAsync()
        {
            return Task.FromResult(_authorizationToken.GetToken(_userId));
        }
    }
}