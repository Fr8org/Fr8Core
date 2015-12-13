using Data.Entities;

namespace Data.Repositories
{
    public interface IAuthorizationTokenRepository
    {
        void Add(AuthorizationTokenDO newToken);
        void Remove(AuthorizationTokenDO token);
        AuthorizationTokenDO FindToken(string userId, int terminalId, int? state);
        AuthorizationTokenDO FindTokenByExternalState(string externalStateToken);
        AuthorizationTokenDO FindTokenByExternalAccount(string externalAccountId);
        AuthorizationTokenDO FindTokenById(string id);
        AuthorizationTokenDO FindTokenByUserId(string userId);
        int Count();
    }
}