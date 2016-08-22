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
        AuthorizationTokenDO FindToken(string userId, Guid terminalId, int? state);
        AuthorizationTokenDO FindTokenByExternalState(string externalStateToken, Guid terminalId);
        AuthorizationTokenDO FindTokenByExternalAccount(string externalAccountId, Guid terminalId, string userId);
        AuthorizationTokenDO FindTokenById(Guid? id);
        int Count();
    }
}