using System;
using System.Collections.Generic;
using System.Linq;
using Data.Repositories.Cache;
using Data.Repositories.Plan;
using Data.Repositories.Security.Entities;

namespace Data.Repositories.Security.StorageImpl.Cache
{
    /// <summary>
    /// Cache service for security objects and role privileges. Every CachedObject has expiration strategy 
    /// and after given period that object is removed from cache. SecurityObjectsCache is defined as Sigleton and it's constructed only on app start.
    /// </summary>
    public class SecurityObjectsCache : ISecurityObjectsCache
    {
        /// <summary>
        /// Main data structure that holds all cached objects. Key is Guid value and holds all Object Identifiers and Object primary keys.
        /// </summary>
        private readonly Dictionary<Guid, CachedObject> _cachedObjects = new Dictionary<Guid, CachedObject>();
        private readonly object _sync = new object();
        private readonly ISecurityCacheExpirationStrategy _expirationStrategy;

        /// <summary>
        /// Wrapper for security cache object 
        /// </summary>
        private class CachedObject
        {
            /// <summary>
            /// All RolePrivileges defined for data object
            /// </summary>
            public IEnumerable<RolePrivilege> RolePrivileges { get; private set; }
            /// <summary>
            /// Expiration token for removing object from cache
            /// </summary>
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
        
        /// <summary>
        /// Get RolePrivileges list from cache. Return empty list when cache is empty
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<RolePrivilege> Get(Guid id)
        {
            CachedObject cachedObject;
            if (!_cachedObjects.TryGetValue(id, out cachedObject))
            {
                return new List<RolePrivilege>();
            }

            return cachedObject.RolePrivileges;
        }

        /// <summary>
        /// Add new role privileges to list, or update rolePrivileg
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rolePrivileges"></param>
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
