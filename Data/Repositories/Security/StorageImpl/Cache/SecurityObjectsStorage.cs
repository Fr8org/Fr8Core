using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Data.Entities;
using Data.Interfaces;
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
        //todo: provide a context like logic for sqlObjectsStorageProvider and connect it to UnitOfWork.SaveChanges()
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

        public ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(string dataObjectId)
        {
            lock (_cache)
            {
                var permissionSet = _cache.GetRecordBasedPermissionSet(dataObjectId);
                if (permissionSet == null)
                {
                    permissionSet = _securityObjectStorageProvider.GetRecordBasedPermissionSetForObject(dataObjectId);
                    if (permissionSet == null) return null; 
                    _cache.AddOrUpdateRecordBasedPermissionSet(dataObjectId, permissionSet);

                    return permissionSet;
                }

                return permissionSet;
            }
        }

        public List<int> GetObjectBasedPermissionSetForObject(string dataObjectId, string dataObjectType, List<string> roleNames)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = new List<int>();
                var roleCollection = uow.AspNetRolesRepository.GetQuery().Where(x=> roleNames.Contains(x.Name) && x.ProfileId.HasValue).ToList();
                foreach (var role in roleCollection)
                {
                    //todo: use cache for profile permission sets
                    result.AddRange(uow.PermissionSetRepository.GetQuery().Where(x => x.ProfileId == role.ProfileId).SelectMany(l => l.Permissions.Select(m => m.Id)).ToList());
                }

                return result;
            }
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
                var rolePermissions = GetRecordBasedPermissionSetForObject(dataObjectId);
                _cache.AddOrUpdateRecordBasedPermissionSet(dataObjectId, rolePermissions);
            }
        }
    }
}
