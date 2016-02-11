using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Data.Entities;
using Data.Interfaces;
using Utilities.Configuration.Azure;

namespace Data.Repositories
{
    public abstract class AuthorizationTokenRepositoryBase : GenericRepository<AuthorizationTokenDO>,
        IAuthorizationTokenRepository, ITrackingChangesRepository
    {
        /*********************************************************************************/
        // Declarations
        /*********************************************************************************/

        private readonly Dictionary<Guid, AuthorizationTokenChangeTracker> _changesTackers =
            new Dictionary<Guid, AuthorizationTokenChangeTracker>();

        private readonly List<AuthorizationTokenDO> _adds = new List<AuthorizationTokenDO>();
        private readonly List<AuthorizationTokenDO> _deletes = new List<AuthorizationTokenDO>();
        private static readonly MemoryCache TokenCache = new MemoryCache("AuthTokenCache");
        private static TimeSpan _expiration = TimeSpan.FromMinutes(10);

        /*********************************************************************************/

        public Type EntityType
        {
            get { return typeof (AuthorizationTokenDO); }
        }

        /*********************************************************************************/
        // Functions
        /*********************************************************************************/

        static AuthorizationTokenRepositoryBase()
        {
            int exp;

            var expStr = CloudConfigurationManager.GetSetting("Cache.AuthorizationTokenRepository.Expiration");

            if (!string.IsNullOrWhiteSpace(expStr) && int.TryParse(expStr, out exp))
            {
                _expiration = TimeSpan.FromMinutes(exp);
            }
        }

        /*********************************************************************************/

        protected AuthorizationTokenRepositoryBase(IUnitOfWork uow)
            : base(uow)
        {
        }

        /*********************************************************************************/

        public IQueryable<AuthorizationTokenDO> GetPublicDataQuery()
        {
            return GetQuery();
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindToken(string userId, int terminalId, int? state)
        {
            AuthorizationTokenDO token;

            if (state == null)
            {
                token = GetQuery().FirstOrDefault(x => x.UserID == userId && x.TerminalID == terminalId);
            }
            else
            {
                token =
                    GetQuery()
                        .FirstOrDefault(
                            x => x.UserID == userId && x.TerminalID == terminalId && x.AuthorizationTokenState == state);
            }

            return EnrichAndTrack(token);
        }

        /*********************************************************************************/

        public int Count()
        {
            return GetQuery().Count();
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenByExternalAccount(string externalAccountId, int terminalId, string userId)
        {
            return EnrichAndTrack(
                GetQuery()
                    .FirstOrDefault(x => x.ExternalAccountId == externalAccountId
                                         && x.TerminalID == terminalId
                                         && x.UserID == userId
                    )
                );
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenById(Guid? id)
        {
            if (id == null)
            {
                return null;
            }

            return FindTokenById(id.Value.ToString());
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenById(string id)
        {
            AuthorizationTokenDO token;

            lock (TokenCache)
            {
                token = (AuthorizationTokenDO) TokenCache.Get(id);
                if (token != null)
                {
                    return token;
                }
            }

            token = EnrichAndTrack(GetQuery().FirstOrDefault(x => x.Id.ToString() == id));

            lock (TokenCache)
            {
                TokenCache.Remove(id);
                TokenCache.Add(new CacheItem(id, token), new CacheItemPolicy
                {
                    SlidingExpiration = _expiration
                });
            }

            return token;

        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenByExternalState(
            string externalStateToken, int terminalId)
        {
            return EnrichAndTrack(
                GetQuery().FirstOrDefault(x => x.TerminalID == terminalId
                    && x.ExternalStateToken == externalStateToken)
            );
        }

        /*********************************************************************************/

        private AuthorizationTokenDO EnrichAndTrack(AuthorizationTokenDO token)
        {
            if (token == null)
            {
                return null;
            }

            AuthorizationTokenChangeTracker changeTracker;

            if (!_changesTackers.TryGetValue(token.Id, out changeTracker))
            {
                token.Token = QuerySecurePart(token.Id);

                _changesTackers[token.Id] = new AuthorizationTokenChangeTracker(token.Token, token);
            }

            return token;
        }

        /*********************************************************************************/

        private AuthorizationTokenChangeTracker Track(AuthorizationTokenDO token)
        {
            AuthorizationTokenChangeTracker changeTracker;

            if (!_changesTackers.TryGetValue(token.Id, out changeTracker))
            {
                changeTracker = new AuthorizationTokenChangeTracker(token.Token, token);
                _changesTackers[token.Id] = changeTracker;
            }

            return changeTracker;
        }

        /*********************************************************************************/

        public void TrackAdds(IEnumerable<object> entities)
        {
            foreach (var entity in entities.OfType<AuthorizationTokenDO>())
            {
                _adds.Add(entity);
                Track(entity).ResetChanges();
            }
        }

        /*********************************************************************************/

        public void TrackDeletes(IEnumerable<object> entities)
        {
            foreach (var entity in entities.OfType<AuthorizationTokenDO>())
            {
                _deletes.Add(entity);
                _changesTackers.Remove(entity.Id);
            }
        }

        /*********************************************************************************/

        public void TrackUpdates(IEnumerable<object> entities)
        {
        }

        /*********************************************************************************/

        public void SaveChanges()
        {
            ProcessChanges(_adds, _changesTackers.Values.Where(x=>x.HasChanges).Select(x=>x.ActualValue), _deletes);
            
            foreach (var value in _changesTackers)
            {
                if (value.Value.HasChanges)
                {
                    value.Value.ResetChanges();
                }
            }

            lock (TokenCache)
            {
                foreach (var authorizationTokenDo in _deletes)
                {
                    TokenCache.Remove(authorizationTokenDo.Id.ToString("N"));
                }
            }

            _adds.Clear();
            _deletes.Clear();
        }

        /*********************************************************************************/

        protected abstract void ProcessChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes);
        protected abstract string QuerySecurePart(Guid id);

        /*********************************************************************************/
    }
}