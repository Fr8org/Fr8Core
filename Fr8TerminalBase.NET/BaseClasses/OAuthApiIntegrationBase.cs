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

        public async Task<TResponse> ApiCall<TResponse>(Func<AuthorizationToken, Task<TResponse>> apiCall, AuthorizationToken auth)
        {
            if (auth.ExpiresAt > DateTime.UtcNow)
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

        protected abstract Task<AuthorizationToken> RefreshToken(AuthorizationToken auth);

        protected virtual bool IsExpiredAccessTokenException(Exception ex)
        {
            var restfulServiceException = ex as RestfulServiceException;
            if (restfulServiceException == null)
            {
                return false;
            }
            return restfulServiceException.StatusCode == (int)HttpStatusCode.Unauthorized
                   || restfulServiceException.Message.Contains("invalid_grant")
                   || restfulServiceException.Message.Contains("expired access/refresh token");
        }
    }
}