using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;

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

        bool TryAssignAuthToken(IUnitOfWork uow, string userId, Guid terminalId, ActivityDO activityDO,
            out AuthorizationTokenDO curAuthToken);     

        /// <summary>
        /// Updates token in database
        /// </summary>
        /// <param name="">Be careful, not all fields may be filled</param>
        void RenewToken(AuthorizationTokenDTO token);

        /// <summary>
        /// Send authentication code to your mobile phone number that is used later for receiving access token
        /// </summary>
        /// <param name="account"></param>
        /// <param name="terminal"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        Task<PhoneNumberCredentialsDTO> SendAuthenticationCodeToMobilePhone(Fr8AccountDO account, TerminalDO terminal, string phoneNumber);

        /// <summary>
        /// Verify Code send to user mobile phone, authenticate and return access token
        /// </summary>
        /// <param name="account"></param>
        /// <param name="terminal"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="verificationCode"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<AuthenticateResponse> VerifyCodeAndGetAccessToken(Fr8AccountDO account, TerminalDO terminal, string phoneNumber, string verificationCode, string clientId, string clientName);
    }
}
