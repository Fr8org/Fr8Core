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

        public int InsertRolePrivilege(RolePrivilege rolePrivilege)
        {
            return _securityObjectStorageProvider.InsertRolePrivilege(rolePrivilege);
        }

        public int UpdateRolePrivilege(RolePrivilege rolePrivilege)
        {
            return _securityObjectStorageProvider.UpdateRolePrivilege(rolePrivilege);
        }

        public int InsertObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string dataObjectType, string propertyName = null)
        {
            var affectedRows = _securityObjectStorageProvider.InsertObjectRolePrivilege(dataObjectId, rolePrivilegeId, dataObjectType, propertyName);

            InvokeCacheUpdate(dataObjectId);

            return affectedRows;
        }

        public int RemoveObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string propertyName = null)
        {
            var affectedRows = _securityObjectStorageProvider.RemoveObjectRolePrivilege(dataObjectId, rolePrivilegeId, propertyName);

            InvokeCacheUpdate(dataObjectId);

            return affectedRows;
        }

        public ObjectRolePrivilegesDO GetRolePrivilegesForSecuredObject(string dataObjectId)
        {
            lock (_cache)
            {
                var rolePrivileges = _cache.Get(dataObjectId);
                if (rolePrivileges == null)
                {
                    rolePrivileges = _securityObjectStorageProvider.GetRolePrivilegesForSecuredObject(dataObjectId);
                    _cache.AddOrUpdate(dataObjectId, rolePrivileges);
                }

                return rolePrivileges;
            }
        }

        public List<RolePrivilege> GetRolePrivilegesForFr8Account(Guid fr8AccountId)
        {
            return _securityObjectStorageProvider.GetRolePrivilegesForFr8Account(fr8AccountId);
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
                //update cache with new ObjectRolePrivilege
                var rolePrivileges = GetRolePrivilegesForSecuredObject(dataObjectId);
                _cache.AddOrUpdate(dataObjectId, rolePrivileges);
            }
        }
    }
}
