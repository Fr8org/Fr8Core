using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Fr8.Infrastructure.Communication;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.BaseClasses
{
    public abstract class OAuthApiIntegrationBase : IOAuthApiIntegration
    {
        private readonly IHubCommunicator _hubCommunicator;

        protected OAuthApiIntegrationBase(IHubCommunicator hubCommunicator)
        {
            if (hubCommunicator == null)
            {
                throw new ArgumentNullException(nameof(hubCommunicator));
            }
            _hubCommunicator = hubCommunicator;
        }

        /// <summary>
        /// Perform specified call that will be provided with specified authorization token. If the auth token provided is expired, tries to refresh it and repeat the call
        /// </summary>
        public async Task<TResponse> ApiCall<TResponse>(Func<AuthorizationToken, Task<TResponse>> apiCall, AuthorizationToken auth)
        {
            if (auth.ExpiresAt != null && auth.ExpiresAt < DateTime.UtcNow)
            {
                auth = await RefreshTokenImpl(auth);
            }
            try
            {
                return await apiCall(auth).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!IsExpiredAccessTokenException(ex))
                {
                    throw;
                }
                auth = await RefreshTokenImpl(auth).ConfigureAwait(false);
                return await apiCall(auth).ConfigureAwait(false);
            }
        }

        private async Task<AuthorizationToken> RefreshTokenImpl(AuthorizationToken auth)
        {
            AuthorizationToken result;
            try
            {
                result = await RefreshToken(auth);
            }
            catch
            {
                throw new AuthorizationTokenExpiredOrInvalidException();
            }
            await _hubCommunicator.RenewToken(Mapper.Map<AuthorizationTokenDTO>(result));
            return result;
        }
        /// <summary>
        /// Performs refresh of access token stored in specified auth token and returns new access token
        /// </summary>
        protected abstract Task<AuthorizationToken> RefreshToken(AuthorizationToken auth);
        /// <summary>
        /// Checks whether specified exception signals about issues connected with expired access token
        /// </summary>
        protected virtual bool IsExpiredAccessTokenException(Exception ex)
        {
            if (ex.Message.Contains("invalid_grant") || ex.Message.Contains("expired access/refresh token"))
            {
                return true;
            }
            var restfulServiceException = ex as RestfulServiceException;
            if (restfulServiceException == null)
            {
                return false;
            }
            return restfulServiceException.StatusCode == (int)HttpStatusCode.Unauthorized;
        }
    }
}