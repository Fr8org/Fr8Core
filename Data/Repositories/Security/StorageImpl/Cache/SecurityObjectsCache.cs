using System;
using System.Collections.Generic;
using System.Linq;
using Data.Repositories.Cache;
using Data.Repositories.Plan;
using Data.Repositories.Security.Entities;

namespace Data.Repositories.Security.StorageImpl.Cache
{
    public class SecurityObjectsCache : ISecurityObjectsCache
    {
        private readonly Dictionary<Guid, CachedObject> _cachedObjects = new Dictionary<Guid, CachedObject>();
        private readonly object _sync = new object();
        private readonly ISecurityCacheExpirationStrategy _expirationStrategy;

        private class CachedObject
        {
            public IEnumerable<RolePrivilege> RolePrivileges { get; private set; }
            public IExpirationToken Expiration { get; set; }

            public CachedObject(IEnumerable<RolePrivilege> rolePrivileges, IExpirationToken expiration)
            {
                RolePrivileges = rolePrivileges;
                Expiration = expiration;
            }
        }

        public SecurityObjectsCache(ISecurityCacheExpirationStrategy expirationStrategy)
        {
            _expirationStrategy = expirationStrategy;
            expirationStrategy.SetExpirationCallback(RemoveExpiredPlans);
        }
        
        public IEnumerable<RolePrivilege> Get(Guid id)
        {
            CachedObject cachedObject;
            if (!_cachedObjects.TryGetValue(id, out cachedObject))
            {
                return new List<RolePrivilege>();
            }

            return cachedObject.RolePrivileges;
        }

        public void AddOrUpdate(Guid id, IEnumerable<RolePrivilege> rolePrivileges)
        {
            lock (_sync)
            {
                CachedObject cachedObject;
                if (!_cachedObjects.TryGetValue(id, out cachedObject))
                {
                    AddToCache(id, rolePrivileges);
                }
                else
                {
                    UpdateCache(id, rolePrivileges);
                }
            }
        }

        private void AddToCache(Guid id, IEnumerable<RolePrivilege> rolePrivileges)
        {
            var expirOn = _expirationStrategy.NewExpirationToken();
            var cachedObject = new CachedObject(rolePrivileges, expirOn);

            _cachedObjects.Add(id, cachedObject);
        }

        private void UpdateCache(Guid id, IEnumerable<RolePrivilege> rolePrivileges)
        {
            var expirOn = _expirationStrategy.NewExpirationToken();
            var cachedObject = new CachedObject(rolePrivileges, expirOn);

            _cachedObjects[id] = cachedObject;
        }

        private void DropCachedObject(Guid id)
        {
            CachedObject cachedObject;
            if (!_cachedObjects.TryGetValue(id, out cachedObject))
            {
                return;
            }

            _cachedObjects.Remove(id);
        }

        private void RemoveExpiredPlans()
        {
            lock (_sync)
            {
                foreach (var objectExpiration in _cachedObjects.ToArray())
                {
                    if (objectExpiration.Value.Expiration.IsExpired())
                    {
                        _cachedObjects.Remove(objectExpiration.Key);
                    }
                }
            }
        }
    }
}
