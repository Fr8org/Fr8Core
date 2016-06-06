using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Web;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Services.Authorization
{
    public class GoogleAuthorizer : IOAuthAuthorizer
    {
        class AuthResultAdapter : IOAuthAuthorizationResult
        {
            private readonly AuthorizationCodeWebApp.AuthResult _result;
            
            public AuthResultAdapter(AuthorizationCodeWebApp.AuthResult result)
            {
                _result = result;
            }

            public bool IsAuthorized => _result.Credential != null;

            public string RedirectUri => _result.RedirectUri;
        }

        public FlowMetadata CreateFlowMetadata(string userId, string email = null, string callbackUrl = null)
        {
            return new AppFlowMetadata(userId, email, callbackUrl);
        }

        public FlowMetadata CreateFlowMetadata(GoogleAuthDTO userId, string email = null, string callbackUrl = null)
        {
            return new AppFlowMetadata(userId, email, callbackUrl);
        }

        private IAuthorizationCodeFlow CreateFlow(string userId)
        {
            return CreateFlowMetadata(userId).Flow;
        }

        public async Task<IOAuthAuthorizationResult> AuthorizeAsync(string userId, string email, string callbackUrl, string currentUrl, CancellationToken cancellationToken)
        {
            var flowMetadata = CreateFlowMetadata(userId, email, callbackUrl);
            var result = await new AuthorizationCodeWebApp(flowMetadata.Flow, flowMetadata.AuthCallback, currentUrl)
                                   .AuthorizeAsync(userId, CancellationToken.None);
            return new AuthResultAdapter(result);
        }

        public async Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var tokenResponse = await flow.LoadTokenAsync(userId, cancellationToken);
            try
            {
                await flow.RevokeTokenAsync(userId, tokenResponse.AccessToken, cancellationToken);
            }
            catch (TokenResponseException ex)
            {
                if (string.Equals(ex.Error.Error, "invalid_token", StringComparison.Ordinal))
                {
                    // token is invalid likely because it has been revoked from google site
                }
                else
                {
                    throw;
                }
            }
            await flow.DeleteTokenAsync(userId, cancellationToken);
        }

        public async Task RefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var tokenResponse = await flow.LoadTokenAsync(userId, cancellationToken);
            if (tokenResponse == null)
                throw new UnauthorizedAccessException($"No token found for user '{userId}'.");
            await flow.RefreshTokenAsync(userId, tokenResponse.RefreshToken, cancellationToken);
        }

        public async Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var tokenResponse = await flow.LoadTokenAsync(userId, cancellationToken);
            if (tokenResponse == null)
                throw new UnauthorizedAccessException($"No token found for user '{userId}'.");
            return tokenResponse.AccessToken;
        }

        public async Task<string> GetRefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var tokenResponse = await flow.LoadTokenAsync(userId, cancellationToken);
            if (tokenResponse == null)
                throw new UnauthorizedAccessException($"No token found for user '{userId}'.");
            return tokenResponse.RefreshToken;
        }
    }
}
