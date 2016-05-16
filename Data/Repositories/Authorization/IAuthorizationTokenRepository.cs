using System;
using System.Linq;
using Data.Entities;

namespace Data.Repositories
{
    public interface IAuthorizationTokenRepository
    {
        IQueryable<AuthorizationTokenDO> GetPublicDataQuery();
        void Add(AuthorizationTokenDO newToken);
        void Remove(AuthorizationTokenDO token);
        AuthorizationTokenDO FindToken(string userId, int terminalId, int? state);
        AuthorizationTokenDO FindTokenByExternalState(string externalStateToken, int terminalId);
        AuthorizationTokenDO FindTokenByExternalAccount(string externalAccountId, int terminalId, string userId);
        AuthorizationTokenDO FindTokenById(Guid? id);
        int Count();
    }
}