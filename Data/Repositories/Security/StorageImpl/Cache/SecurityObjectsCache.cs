using System.Collections.Generic;
using System.Linq;
using Data.Repositories.Cache;
using Data.Repositories.Security.Entities;

namespace Data.Repositories.Security.StorageImpl.Cache
{
    /// <summary>
    /// Cache service for security objects and role permissions. Every CachedObject has expiration strategy 
    /// and after given period that object is removed from cache. SecurityObjectsCache is defined as Sigleton and it's constructed only on app start.
    /// </summary>
    public class SecurityObjectsCache : ISecurityObjectsCache
    {
        /// <summary>
        /// Main data structure that holds all cached objects. Key is Guid value and holds all Object Identifiers and Object primary keys.
        /// </summary>
        private readonly Dictionary<string, CachedObject> _cachedObjects = new Dictionary<string, CachedObject>();
        private readonly object _sync = new object();
        private readonly ISecurityCacheExpirationStrategy _expirationStrategy;

        /// <summary>
        /// Wrapper for security cache object 
        /// </summary>
        private class CachedObject
        {
            /// <summary>
            /// All RolePermissions defined for data object
            /// </summary>
            public ObjectRolePermissionsDO RolePermissions { get; private set; }
            /// <summary>
            /// Expiration token for removing object from cache
            /// </summary>
            public IExpirationToken Expiration { get; set; }

            public CachedObject(ObjectRolePermissionsDO rolePermissions, IExpirationToken expiration)
            {
                RolePermissions = rolePermissions;
                Expiration = expiration;
            }
        }

        public SecurityObjectsCache(ISecurityCacheExpirationStrategy expirationStrategy)
        {
            _expirationStrategy = expirationStrategy;
            expirationStrategy.SetExpirationCallback(RemoveExpiredPlans);
        }

        /// <summary>
        /// Get RolePermissions list from cache. Return empty list when cache is empty
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ObjectRolePermissionsDO Get(string id)
        {
            lock (_sync)
            {
                CachedObject cachedObject;
                if (!_cachedObjects.TryGetValue(id, out cachedObject))
                {
                    return null;
                }

                return cachedObject.RolePermissions;
            }
        }

        /// <summary>
        /// Add new role permissions to list, or update role permissions
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rolePermissions"></param>
        public void AddOrUpdate(string id, ObjectRolePermissionsDO rolePermissions)
        {
            lock (_sync)
            {
                CachedObject cachedObject;
                if (!_cachedObjects.TryGetValue(id, out cachedObject))
                {
                    var expirOn = _expirationStrategy.NewExpirationToken();
                    cachedObject = new CachedObject(rolePermissions, expirOn);

                    _cachedObjects.Add(id, cachedObject);
                }
                else
                {
                    var expirOn = _expirationStrategy.NewExpirationToken();
                    cachedObject = new CachedObject(rolePermissions, expirOn);

                    _cachedObjects[id] = cachedObject;
                }
            }
        }

        private void DropCachedObject(string id)
        {
            lock (_sync)
            {
                _cachedObjects.Remove(id);
            }
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
