using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IAuthorization
    {
        void PrepareAuthToken(IUnitOfWork uow, ActivityDTO activityDTO);

        Task<AuthenticateResponse> AuthenticateInternal(Fr8AccountDO account, TerminalDO terminal,
            string domain, string username, string password, bool isDemoAccount = false);

        Task<AuthenticateResponse> GetOAuthToken(TerminalDO terminal, ExternalAuthenticationDTO externalAuthDTO);

        Task<ExternalAuthUrlDTO> GetOAuthInitiationURL(Fr8AccountDO user, TerminalDO terminal);

        void AddAuthenticationCrate(ActivityDTO activityDTO, int authType);

        void RemoveAuthenticationCrate(ActivityDTO activityDTO);

        bool ValidateAuthenticationNeeded(IUnitOfWork uow, string userId, ActivityDTO curActionDTO);

        void RevokeTokenIfNeeded(IUnitOfWork uow, ActivityDTO activityDTO);

        void InvalidateToken(IUnitOfWork uow, string userId, ActivityDTO curActivityDto);

        IEnumerable<AuthorizationTokenDO> GetAllTokens(string accountId);

        void SetMainToken(string userId, Guid authTokenId);

        void GrantToken(Guid actionId, Guid authTokenId);

        void RevokeToken(string accountId, Guid authTokenId);
    }
}
