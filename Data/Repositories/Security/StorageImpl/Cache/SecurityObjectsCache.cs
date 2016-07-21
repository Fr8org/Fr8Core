using System.Collections.Generic;
using System.Linq;
using Data.Entities;
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
        private readonly Dictionary<string, CachedPermissionSetObject> _cachedPermissionSetObjects = new Dictionary<string, CachedPermissionSetObject>();
        /// <summary>
        /// Main data structure that hold all cached profiles.
        /// </summary>
        private readonly Dictionary<string, CachedProfileObject> _cachedProfileObjects = new Dictionary<string, CachedProfileObject>();

        private readonly object _sync = new object();
        private readonly object _syncProfiles = new object();
        private readonly ISecurityCacheExpirationStrategy _expirationStrategy;

        /// <summary>
        /// Wrapper for security cache object for one permission set. Used for Record Based Security
        /// </summary>
        private class CachedPermissionSetObject
        {
            /// <summary>
            /// All RolePermissions defined for data object
            /// </summary>
            public ObjectRolePermissionsWrapper ObjectRolePermissionsWrapper { get; private set; }
            /// <summary>
            /// Expiration token for removing object from cache
            /// </summary>
            public IExpirationToken Expiration { get; set; }

            public CachedPermissionSetObject(ObjectRolePermissionsWrapper rolePermissions, IExpirationToken expiration)
            {
                ObjectRolePermissionsWrapper = rolePermissions;
                Expiration = expiration;
            }
        }

        /// <summary>
        /// Wrapper for security cache object for Profiles. Used for Object Based Security
        /// </summary>
        private class CachedProfileObject
        {
            /// <summary>
            /// All RolePermissions defined for data object
            /// </summary>
            public List<PermissionSetDO> PermissionSets { get; private set; }
            /// <summary>
            /// Expiration token for removing object from cache
            /// </summary>
            public IExpirationToken Expiration { get; set; }

            public CachedProfileObject(List<PermissionSetDO> permissions, IExpirationToken expiration)
            {
                PermissionSets = permissions;
                Expiration = expiration;
            }
        }

        public SecurityObjectsCache(ISecurityCacheExpirationStrategy expirationStrategy)
        {
            _expirationStrategy = expirationStrategy;
            expirationStrategy.SetExpirationCallback(RemoveExpiredPermissionSets);
        }

        /// <summary>
        /// Get PermissionSet for a objectfrom cache. Return null object when cache is empty
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ObjectRolePermissionsWrapper GetRecordBasedPermissionSet(string id)
        {
            lock (_sync)
            {
                CachedPermissionSetObject cachedObject;
                if (!_cachedPermissionSetObjects.TryGetValue(id, out cachedObject))
                {
                    return null;
                }

                return cachedObject.ObjectRolePermissionsWrapper;
            }
        }


        /// <summary>
        /// Get All  PermissionSets for a profile cache. Return null object when cache is empty
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<PermissionSetDO> GetProfilePermissionSets(string id)
        {
            lock (_syncProfiles)
            {
                CachedProfileObject cachedObject;
                if (!_cachedProfileObjects.TryGetValue(id, out cachedObject))
                {
                    return new List<PermissionSetDO>();
                }

                return cachedObject.PermissionSets;
            }
        }

        /// <summary>
        /// Add new role permissions to list, or update role permissions
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rolePermissions"></param>
        public void AddOrUpdateRecordBasedPermissionSet(string id, ObjectRolePermissionsWrapper rolePermissions)
        {
            lock (_sync)
            {
                CachedPermissionSetObject cachedObject;
                if (!_cachedPermissionSetObjects.TryGetValue(id, out cachedObject))
                {
                    var expirOn = _expirationStrategy.NewExpirationToken();
                    cachedObject = new CachedPermissionSetObject(rolePermissions, expirOn);

                    _cachedPermissionSetObjects.Add(id, cachedObject);
                }
                else
                {
                    var expirOn = _expirationStrategy.NewExpirationToken();
                    cachedObject = new CachedPermissionSetObject(rolePermissions, expirOn);

                    _cachedPermissionSetObjects[id] = cachedObject;
                }
            }
        }

        /// <summary>
        /// Add new permissions for profile, or update permissions
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rolePermissions"></param>
        public void AddOrUpdateProfile(string id, List<PermissionSetDO> rolePermissions)
        {
            lock (_sync)
            {
                CachedProfileObject cachedObject;
                if (!_cachedProfileObjects.TryGetValue(id, out cachedObject))
                {
                    var expirOn = _expirationStrategy.NewExpirationToken();
                    cachedObject = new CachedProfileObject(rolePermissions, expirOn);

                    _cachedProfileObjects.Add(id, cachedObject);
                }
                else
                {
                    var expirOn = _expirationStrategy.NewExpirationToken();
                    cachedObject = new CachedProfileObject(rolePermissions, expirOn);

                    _cachedProfileObjects[id] = cachedObject;
                }
            }
        }

        private void RemoveExpiredPermissionSets()
        {
            lock (_sync)
            {
                foreach (var objectExpiration in _cachedPermissionSetObjects.ToArray())
                {
                    if (objectExpiration.Value.Expiration.IsExpired())
                    {
                        _cachedPermissionSetObjects.Remove(objectExpiration.Key);
                    }
                }
            }

            lock (_syncProfiles)
            {
                foreach (var objectExpiration in _cachedProfileObjects.ToArray())
                {
                    if (objectExpiration.Value.Expiration.IsExpired())
                    {
                        _cachedProfileObjects.Remove(objectExpiration.Key);
                    }
                }
            }
        }
    }
}
