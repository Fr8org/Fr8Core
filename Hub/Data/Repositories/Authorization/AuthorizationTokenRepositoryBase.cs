using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using Data.Entities;
using Data.Repositories.Authorization;
using Fr8.Infrastructure.Utilities.Configuration;

namespace Data.Repositories
{
    public abstract partial class AuthorizationTokenRepositoryBase : IAuthorizationTokenRepository
    {
        /*********************************************************************************/
        // Declarations
        /*********************************************************************************/

        private static readonly MemoryCache TokenCache = new MemoryCache("AuthTokenCache");
        private static readonly MemoryCache TokenSecureCache = new MemoryCache("AuthTokenSecureCache");
        private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(10);

        private readonly Dictionary<Guid, AuthorizationTokenChangeTracker> _changesTackers = new Dictionary<Guid, AuthorizationTokenChangeTracker>();
        private readonly IAuthorizationTokenStorageProvider _storageProvider;
        
        /*********************************************************************************/
        // Functions
        /*********************************************************************************/

        static AuthorizationTokenRepositoryBase()
        {
            int exp;

            var expStr = CloudConfigurationManager.GetSetting("Cache.AuthorizationTokenRepository.Expiration");

            if (!string.IsNullOrWhiteSpace(expStr) && int.TryParse(expStr, out exp))
            {
                Expiration = TimeSpan.FromMinutes(exp);
            }
        }

        /*********************************************************************************/

        protected AuthorizationTokenRepositoryBase(IAuthorizationTokenStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
           
        }

        /*********************************************************************************/
        
        public int Count()
        {
            return _storageProvider.GetQuery().Count();
        }

        /*********************************************************************************/

        public void Add(AuthorizationTokenDO authorizationTokenDo)
        {
            AuthorizationTokenChangeTracker tracker;

            if (_changesTackers.TryGetValue(authorizationTokenDo.Id, out tracker))
            {
                if (!ReferenceEquals(tracker.ActualValue, authorizationTokenDo))
                {
                    throw new InvalidOperationException($"AuthorizationToken with Id = {authorizationTokenDo.Id} was already added");
                }
               
                return;
            }

            Track(authorizationTokenDo, EntityState.Added);
        }

        /*********************************************************************************/

        public void Remove(AuthorizationTokenDO authorizationTokenDo)
        {
            Track(authorizationTokenDo, EntityState.Deleted);
        }

        /*********************************************************************************/

        public IQueryable<AuthorizationTokenDO> GetPublicDataQuery()
        {
            return InterceptingProvider.Intercept(_storageProvider.GetQuery(), this);
        }
        
        /*********************************************************************************/

        public AuthorizationTokenDO FindToken(string userId, Guid terminalId, int? state)
        {
            AuthorizationTokenDO token;

            if (state == null)
            {
                token = _storageProvider.GetQuery().FirstOrDefault(x => x.UserID == userId && x.TerminalID == terminalId);
            }
            else
            {
                token = _storageProvider.GetQuery().FirstOrDefault(x => x.UserID == userId && x.TerminalID == terminalId && x.AuthorizationTokenState == state);
            }

            return SyncAndLoadSecretData(token);
        }
        
        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenByExternalAccount(string externalAccountId, Guid terminalId, string userId)
        {
            return SyncAndLoadSecretData(_storageProvider.GetQuery()
                                                                      .FirstOrDefault(x => x.ExternalAccountId == externalAccountId
                                                                      && x.TerminalID == terminalId
                                                                      && x.UserID == userId));
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenByExternalState(string externalStateToken, Guid terminalId)
        {
            return SyncAndLoadSecretData(_storageProvider.GetQuery()
                                                                      .FirstOrDefault(x => x.TerminalID == terminalId
                                                                      && x.ExternalStateToken == externalStateToken));
        }

        /*********************************************************************************/

        public AuthorizationTokenDO FindTokenById(Guid? id)
        {
            if (id == null)
            {
                return null;
            }

            return Sync(id, () => _storageProvider.GetByKey(id.Value), true);
        }

        /*********************************************************************************/

        private AuthorizationTokenDO SyncAndLoadSecretData(AuthorizationTokenDO token)
        {
            return Sync(token?.Id, () => token, true);
        }

        /*********************************************************************************/

        private AuthorizationTokenDO Sync(AuthorizationTokenDO token)
        {
            return Sync(token?.Id, () => token, false);
        }

        /*********************************************************************************/

        private AuthorizationTokenDO Sync(Guid? id, Func<AuthorizationTokenDO> fallback, bool querySecureData)
        {
            if (id == null)
            {
                return null;
            }

            AuthorizationTokenChangeTracker tracker;

            // if we've already loaded this before (local cache)
            if (_changesTackers.TryGetValue(id.Value, out tracker))
            {
                if (tracker.State == EntityState.Deleted)
                {
                    return null;
                }

                if (querySecureData && tracker.ActualValue.Token == null)
                {
                    tracker.InjectSecretData(QuerySecureDataCached(id.Value));
                }

                return tracker.ActualValue;
            }

            // try load from cache (global cache)
            AuthorizationTokenDO token;
            var cacheKey = GetCacheKey(id.Value);

            lock (TokenCache)
            {
                token = (AuthorizationTokenDO)TokenCache.Get(cacheKey);
            }

            // load from the db
            if (token == null)
            {
                token = fallback();

                lock (TokenCache)
                {
                    // double check: if someone has written value to cache while we were quierying data
                    // use value from cache for consistency
                    var tokenFromCache = (AuthorizationTokenDO)TokenCache.Get(cacheKey);

                    if (tokenFromCache != null)
                    {
                        token = tokenFromCache;
                    }
                    else
                    {
                        UpdateCache(token);
                    }
                }
            }

            token = token?.Clone();

            if (token != null && querySecureData && token.Token == null)
            {
                token.Token = QuerySecureDataCached(id.Value);
            }

            token = Track(token, EntityState.Modified);

            return token;
        }
        
        /*********************************************************************************/

        private string QuerySecureDataCached(Guid? id)
        {
            if (id == null)
            {
                return null;
            }

            var cacheKey = GetCacheKey(id.Value);
            string data;

            lock (TokenCache)
            {
                data = (string)TokenSecureCache[cacheKey];

                if (data != null)
                {
                    return data;
                }
            }

            data = QuerySecurePart(id.Value);

            lock (TokenCache)
            {
                // double check: if someone has written value to cache while we were quierying data
                // use value from cache for consistency
                var dataFromCache = (string)TokenSecureCache[cacheKey];

                if (dataFromCache != null)
                {
                    return dataFromCache;
                }

                UpdateSecureCache(id.Value, data);
            }

            return data;
        }

        /*********************************************************************************/

        private void UpdateCache(AuthorizationTokenDO authorizationToken)
        {
            var cacheKey = GetCacheKey(authorizationToken.Id);
            
            TokenCache.Remove(cacheKey);
            TokenCache.Add(new CacheItem(cacheKey, authorizationToken.Clone()), new CacheItemPolicy
            {
                SlidingExpiration = Expiration
            });
        }

        /*********************************************************************************/

        private void UpdateSecureCache(Guid id, string data)
        {
            var cacheKey = GetCacheKey(id);

            TokenSecureCache.Remove(cacheKey);

            if (data != null)
            {
                TokenSecureCache.Add(new CacheItem(cacheKey, data), new CacheItemPolicy
                {
                    SlidingExpiration = Expiration
                });
            }
        }

        /*********************************************************************************/

        private string GetCacheKey(Guid tokenId)
        {
            return tokenId.ToString("N");
        }
      
        /*********************************************************************************/

        private AuthorizationTokenDO Track(AuthorizationTokenDO token, EntityState state)
        {
            AuthorizationTokenChangeTracker changeTracker;

            if (token == null)
            {
                return null;
            }

            if (!_changesTackers.TryGetValue(token.Id, out changeTracker))
            {
                changeTracker = new AuthorizationTokenChangeTracker(token, state);

                _changesTackers[token.Id] = changeTracker;
            }
            else
            {
                changeTracker.State = state;
            }
         
            return changeTracker.ActualValue;
        }
        
        /*********************************************************************************/

        public void SaveChanges()
        {
            var securepdates = _changesTackers.Values.Where(x => x.State == EntityState.Modified && x.IsSecureDataChanged).Select(x => x.ActualValue).ToArray();
            var deletes = _changesTackers.Values.Where(x => x.State == EntityState.Deleted).Select(x => x.ActualValue).ToArray();
            var adds = _changesTackers.Values.Where(x => x.State == EntityState.Added).Select(x => x.ActualValue).ToArray();

            foreach (var authorizationTokenChangeTracker in _changesTackers)
            {
                authorizationTokenChangeTracker.Value.DetectChanges();
            }

            lock (TokenCache)
            {
                foreach (var token in _changesTackers.Values.Where(x => x.State == EntityState.Added || x.HasChanges).Select(x => x.ActualValue))
                {
                    UpdateCache(token);
                }

                foreach (var authorizationTokenDo in adds.Concat(securepdates))
                {
                    UpdateSecureCache(authorizationTokenDo.Id, authorizationTokenDo.Token);
                }

                foreach (var authorizationTokenDo in deletes)
                {
                    var key = GetCacheKey(authorizationTokenDo.Id);

                    TokenCache.Remove(key);
                    TokenSecureCache.Remove(key);
                }
            }

            ProcessSecureDataChanges(adds, securepdates, deletes);

            var changes = new AuthorizationTokenChanges();

            changes.Insert.AddRange(adds);
            changes.Delete.AddRange(deletes);

            foreach (var tracker in _changesTackers.Values.Where(x => x.State == EntityState.Modified && x.HasChanges))
            {
                changes.Update.Add(new AuthorizationTokenChanges.ChangedToken(tracker.ActualValue, tracker.Changes));
            }

            _storageProvider.Update(changes);

            foreach (var tracker in _changesTackers.ToArray())
            {
                if (tracker.Value.State == EntityState.Deleted)
                {
                    _changesTackers.Remove(tracker.Key);
                    continue;
                }

                tracker.Value.ResetChanges();
            }
        }

        /*********************************************************************************/

        protected abstract void ProcessSecureDataChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes);
        protected abstract string QuerySecurePart(Guid id);

        /*********************************************************************************/
    }
}