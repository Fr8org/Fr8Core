using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Repositories.Security.Entities;
using StructureMap;

namespace Data.Repositories.Security.StorageImpl.Cache
{
    /// <summary>
    /// Wrapper for security objects storage.  
    /// Implements all members of SecurityObjectStorageProvider and hold reference to cache for security objects
    /// Inside all methods for get security data, at first, we check if that data is found inside cache, if not then we return from security data provider and we update cache with that data
    /// Inside all create/update methods at first we create data inside storage provider, and then we update the cache. 
    /// </summary>
    public class SecurityObjectsStorage : ISecurityObjectsStorageProvider
    {
        private readonly ISecurityObjectsCache _cache;
        private readonly ISecurityObjectsStorageProvider _securityObjectStorageProvider;

        public SecurityObjectsStorage(ISecurityObjectsCache cache, ISecurityObjectsStorageProvider securityObjectStorageProvider)
        {
            _cache = cache;
            _securityObjectStorageProvider = securityObjectStorageProvider;
        }

        public int InsertRolePermission(RolePermission rolePermission)
        {
            return _securityObjectStorageProvider.InsertRolePermission(rolePermission);
        }

        public int UpdateRolePermission(RolePermission rolePermission)
        {
            return _securityObjectStorageProvider.UpdateRolePermission(rolePermission);
        }

        public int InsertObjectRolePermission(string dataObjectId, Guid rolePermissionId, string dataObjectType, string propertyName = null)
        {
            var affectedRows = _securityObjectStorageProvider.InsertObjectRolePermission(dataObjectId, rolePermissionId, dataObjectType, propertyName);

            InvokeCacheUpdate(dataObjectId);

            return affectedRows;
        }

        public int RemoveObjectRolePermission(string dataObjectId, Guid rolePermissionId, string propertyName = null)
        {
            var affectedRows = _securityObjectStorageProvider.RemoveObjectRolePermission(dataObjectId, rolePermissionId, propertyName);

            InvokeCacheUpdate(dataObjectId);

            return affectedRows;
        }

        public ObjectRolePermissionsDO GetRolePermissionsForSecuredObject(string dataObjectId)
        {
            lock (_cache)
            {
                var rolePermissions = _cache.Get(dataObjectId);
                if (rolePermissions == null)
                {
                    rolePermissions = _securityObjectStorageProvider.GetRolePermissionsForSecuredObject(dataObjectId);
                    _cache.AddOrUpdate(dataObjectId, rolePermissions);
                }

                return rolePermissions;
            }
        }

        public List<RolePermission> GetRolePermissionsForFr8Account(Guid fr8AccountId)
        {
            return _securityObjectStorageProvider.GetRolePermissionsForFr8Account(fr8AccountId);
        }

        public void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType)
        {
            _securityObjectStorageProvider.SetDefaultObjectSecurity(dataObjectId, dataObjectType);

            InvokeCacheUpdate(dataObjectId);
        }

        private void InvokeCacheUpdate(string dataObjectId)
        {
            lock (_cache)
            {
                //update cache with new ObjectRolePermissions
                var rolePermissions = GetRolePermissionsForSecuredObject(dataObjectId);
                _cache.AddOrUpdate(dataObjectId, rolePermissions);
            }
        }
    }
}
