using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Repositories.Security.Entities;
using StructureMap;

namespace Data.Repositories.Security.StorageImpl.Cache
{
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

        public int InsertObjectRolePrivilege(Guid dataObjectId, Guid rolePrivilegeId, string dataObjectType)
        {
            var affectedRows = _securityObjectStorageProvider.InsertObjectRolePrivilege(dataObjectId, rolePrivilegeId, dataObjectType);

            //update cache with new ObjectRolePrivilege
            var rolePrivileges = GetRolePrivilegesForSecuredObject(dataObjectId);
            _cache.AddOrUpdate(dataObjectId, rolePrivileges);

            return affectedRows;
        }

        public int RemoveObjectRolePrivilege(Guid dataObjectId, Guid rolePrivilegeId)
        {
            var affectedRows = _securityObjectStorageProvider.RemoveObjectRolePrivilege(dataObjectId, rolePrivilegeId);

            //update cache with new ObjectRolePrivilege
            var rolePrivileges = GetRolePrivilegesForSecuredObject(dataObjectId);
            _cache.AddOrUpdate(dataObjectId, rolePrivileges);

            return affectedRows;
        }

        public IEnumerable<RolePrivilege> GetRolePrivilegesForSecuredObject(Guid dataObjectId)
        {
            var rolePrivileges = _cache.Get(dataObjectId);
            if (!rolePrivileges.Any())
            {
                rolePrivileges = _securityObjectStorageProvider.GetRolePrivilegesForSecuredObject(dataObjectId);
                _cache.AddOrUpdate(dataObjectId, rolePrivileges);
            }

            return rolePrivileges.ToList();
        }

        public IEnumerable<RolePrivilege> GetRolePrivilegesForFr8Account(Guid fr8AccountId)
        {
            return _securityObjectStorageProvider.GetRolePrivilegesForSecuredObject(fr8AccountId);
        }

        public void SetupDefaultSecurityForDataObject(Guid dataObjectId, string dataObjectType)
        {
            _securityObjectStorageProvider.SetupDefaultSecurityForDataObject(dataObjectId, dataObjectType);

            //update cache with new ObjectRolePrivilege
            var rolePrivileges = GetRolePrivilegesForSecuredObject(dataObjectId);
            _cache.AddOrUpdate(dataObjectId, rolePrivileges);
        }
    }
}
