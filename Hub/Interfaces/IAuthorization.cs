using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IAuthorization
    {
        void PrepareAuthToken(ActionDTO actionDTO);

        Task<string> AuthenticateInternal(Fr8AccountDO account, TerminalDO terminal,
            string domain, string username, string password);

        Task<string> GetOAuthToken(TerminalDO terminal, ExternalAuthenticationDTO externalAuthDTO);

        Task<ExternalAuthUrlDTO> GetOAuthInitiationURL(Fr8AccountDO user, ActionDO actionDO);

        void AddAuthenticationCrate(ActionDTO actionDTO, int authType);

        void RemoveAuthenticationCrate(ActionDTO actionDTO);

        bool ValidateAuthenticationNeeded(string userId, ActionDTO curActionDTO);

        void InvalidateToken(string userId, ActionDTO curActionDto);

        IEnumerable<AuthorizationTokenDO> GetAllTokens(string accountId);

        void GrantToken(Guid actionId, Guid uthTokenId);

        void RevokeToken(string accountId, Guid authTokenId);
    }
}
