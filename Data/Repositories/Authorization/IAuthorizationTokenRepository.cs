using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public interface IAuthorizationTokenRepository : IGenericRepository<AuthorizationTokenDO>
    {
        IQueryable<AuthorizationTokenDO> GetPublicDataQuery();
        void Add(AuthorizationTokenDO newToken);
        void Remove(AuthorizationTokenDO token);
        AuthorizationTokenDO FindToken(string userId, int terminalId, int? state);
        AuthorizationTokenDO FindTokenByExternalState(string externalStateToken, int terminalId);
        AuthorizationTokenDO FindTokenByExternalAccount(string externalAccountId, int terminalId, string userId);
        AuthorizationTokenDO FindTokenById(string id);
        int Count();
    }
}