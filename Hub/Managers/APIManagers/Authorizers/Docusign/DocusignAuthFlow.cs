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
using Hub.Managers.APIManagers.Transmitters.Restful;

namespace Hub.Managers.APIManagers.Authorizers.Docusign
{
    class DocuSignAuthFlow
    {
        private readonly string _userId;
        private readonly Authorization _authorizationToken;
        private readonly IRestfulServiceClient _client;

        public string Endpoint { get; set; }
        public string IntegratorKey { get; set; }

        public DocuSignAuthFlow(string userId)
        {
            _userId = userId;
            _authorizationToken = new Authorization();
            _client = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }

        private Uri BuildAbsoluteUrl(string relativeUrl)
        {
            return new Uri(new Uri(Endpoint), relativeUrl);
        }

        private Dictionary<string, string> GetAuthenticationHeader(string accessToken)
        {
            return new Dictionary<string, string>
            {
                { System.Net.HttpRequestHeader.Authorization.ToString(), string.Format("bearer {0}", accessToken) }
            };
        }

        private async Task<bool> LoginAsync(string accessToken)
        {
            try
            {
                await _client.GetAsync<string>(BuildAbsoluteUrl("login_information"), null, GetAuthenticationHeader(accessToken));
            }
            catch {
                return false;
            }

            return true;

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
            string response;
            try
            {
                response = await _client.PostAsync(BuildAbsoluteUrl("oauth2/token"),
                (HttpContent)new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", IntegratorKey),
                    new KeyValuePair<string, string>("username", userName),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("scope", "api"),
                }));
            }
            catch (RestfulServiceException ex)
            {
                throw new OAuthException("Failed to obtain an access token. Credentials must have been incorrect.", ex);
            }
                
            var responseObject = JsonConvert.DeserializeAnonymousType(response, new { access_token = "" });
            _authorizationToken.AddOrUpdateToken(_userId, responseObject.access_token);
        }

        public async Task RevokeTokenAsync()
        {
            var accessToken = await GetTokenAsync();
            if (string.IsNullOrEmpty(accessToken))
                return;
 
            try
            {
                var response = await _client.PostAsync(BuildAbsoluteUrl("oauth2/revoke"),
                (HttpContent)new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", accessToken),
                }));
            }
            catch (RestfulServiceException ex)
            {
                throw new OAuthException("Failed to revoke an access token.", ex);
            }
            _authorizationToken.RemoveToken(_userId);
            
        }

        public Task<string> GetTokenAsync()
        {
            return Task.FromResult(_authorizationToken.GetToken(_userId));
        }
    }
}