using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Security.Entities;
using Data.States;
using Data.States.Templates;
using Fr8.Infrastructure.Data.DataTransferObjects;
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
        private readonly IUnitOfWork _uow;
        //todo: provide a context like logic for sqlObjectsStorageProvider and connect it to UnitOfWork.SaveChanges()
        private readonly ISecurityObjectsStorageProvider _securityObjectStorageProvider;

        public SecurityObjectsStorage(IUnitOfWork uow, ISecurityObjectsCache cache, ISecurityObjectsStorageProvider securityObjectStorageProvider)
        {
            _uow = uow;
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

        public int InsertObjectRolePermission(string currentUserId, Guid dataObjectId, Guid rolePermissionId, string dataObjectType, string propertyName = null)
        {
            var affectedRows = _securityObjectStorageProvider.InsertObjectRolePermission(currentUserId, dataObjectId, rolePermissionId, dataObjectType, propertyName);

            InvokeCacheUpdate(dataObjectId, dataObjectType);

            return affectedRows;
        }

        public int RemoveObjectRolePermission(Guid dataObjectId, Guid rolePermissionId, string propertyName = null)
        {
            var affectedRows = _securityObjectStorageProvider.RemoveObjectRolePermission(dataObjectId, rolePermissionId, propertyName);

            InvokeCacheUpdate(dataObjectId, string.Empty);

            return affectedRows;
        }

        public ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(Guid dataObjectId, string dataObjectType)
        {
            lock (_cache)
            {
                var permissionSet = _cache.GetRecordBasedPermissionSet($"{dataObjectType}:{dataObjectId}");
                if (permissionSet != null) return permissionSet;

                permissionSet = _securityObjectStorageProvider.GetRecordBasedPermissionSetForObject(dataObjectId, dataObjectType);
                if (!permissionSet.RolePermissions.Any() && !permissionSet.Properties.Any())
                    return new ObjectRolePermissionsWrapper();

                _cache.AddOrUpdateRecordBasedPermissionSet($"{dataObjectType}:{dataObjectId}", permissionSet);
                return permissionSet;
            }
        }

        public List<PermissionDTO> GetAllPermissionsForUser(Guid profileId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = new List<PermissionDTO>();
                lock (_cache)
                {
                    var permissionSets = _cache.GetProfilePermissionSets(profileId.ToString());
                    if (!permissionSets.Any())
                    {
                        permissionSets = uow.PermissionSetRepository.GetQuery().Where(x => x.ProfileId == profileId).ToList();
                        var clonedPermissionSets = permissionSets.Select(x => x.Clone()).ToList();
                        _cache.AddOrUpdateProfile(profileId.ToString(), clonedPermissionSets);
                    }
                    foreach (var set in permissionSets)
                    {
                        result.AddRange(set.Permissions.Select(x=> new PermissionDTO() { Permission = x.Id, ObjectType = set.ObjectType}).ToList());
                    }
                }

                return result;
            }
        }

        public List<int> GetObjectBasedPermissionSetForObject(Guid dataObjectId, string dataObjectType, Guid profileId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var result = new List<int>();
                lock (_cache)
                {
                    var permissionSets = _cache.GetProfilePermissionSets(profileId.ToString());
                    if (!permissionSets.Any())
                    {
                        permissionSets = uow.PermissionSetRepository.GetQuery().Where(x => x.ProfileId == profileId).ToList();
                        var clonedPermissionSets = permissionSets.Select(x => x.Clone()).ToList();
                        _cache.AddOrUpdateProfile(profileId.ToString(), clonedPermissionSets);
                    }
                    result.AddRange(permissionSets.Where(x => x.ObjectType == dataObjectType).SelectMany(l => l.Permissions.Select(m => m.Id)).ToList());
                }

                return result;
            }
        }

        public void SetDefaultRecordBasedSecurityForObject(string currentUserId, string roleName, Guid dataObjectId, string dataObjectType,
            Guid rolePermissionId, int? organizationId, List<PermissionType> customPermissionTypes = null)
        {
            if (rolePermissionId == Guid.Empty)
            {
                var permissionSet = GetOrCreateDefaultSecurityPermissionSet(dataObjectType, customPermissionTypes);

                if (permissionSet == null)
                {
                    throw new NullReferenceException("System failed to find a permission set for default security.");
                }

                var rolePermission = GetRolePermission(roleName, permissionSet.Id);
                if (rolePermission == null)
                {
                    InsertRolePermission(new RolePermission() { PermissionSet = permissionSet, Role = new RoleDO { RoleName = roleName } });
                    rolePermission = GetRolePermission(roleName, permissionSet.Id);
                }
                rolePermissionId = rolePermission.Id;
            }

            //dedicate one permission set for a dataType and OwnerOfCurrent   
            _securityObjectStorageProvider.SetDefaultRecordBasedSecurityForObject(currentUserId, roleName, dataObjectId, dataObjectType, rolePermissionId, organizationId);

            InvokeCacheUpdate(dataObjectId, dataObjectType);
        }
        
        public RolePermission GetRolePermission(string roleName, Guid permissionSetId)
        {
            return _securityObjectStorageProvider.GetRolePermission(roleName, permissionSetId);
        }

        public List<string> GetAllowedUserRolesForSecuredObject(Guid objectId, string objectType)
        {
            return _securityObjectStorageProvider.GetAllowedUserRolesForSecuredObject(objectId, objectType);
        }

        private PermissionSetDO GetOrCreateDefaultSecurityPermissionSet(string dataObjectType, List<PermissionType> customPermissionTypes = null)
        {
            // this method use injected UnitOfWork instance in order to work properly once being called from MigrationConfiguration
            var defaultPermissions = customPermissionTypes?.Select(x => (int) x).ToArray() 
                                       ?? new[] { (int)PermissionType.ReadObject, (int)PermissionType.EditObject, (int)PermissionType.CreateObject, (int)PermissionType.DeleteObject, (int)PermissionType.RunObject };

            var defaultPermissionsCount = defaultPermissions.Length;
            //check for existing permission set with this default permissions
            var permissionSet = _uow.PermissionSetRepository.GetQuery().FirstOrDefault(x => x.ObjectType == dataObjectType && x.Permissions.Count == defaultPermissionsCount &&
                                         x.Permissions.All(t => defaultPermissions.Contains(t.Id)));

            if (permissionSet != null)
            {
                return permissionSet;
            }

            permissionSet = new PermissionSetDO()
            {
                Id = Guid.NewGuid(),
                Name = Roles.OwnerOfCurrentObject,
                ObjectType = dataObjectType,
            };
            var repo = new GenericRepository<_PermissionTypeTemplate>(_uow);
            foreach (var permission in defaultPermissions)
            {
                permissionSet.Permissions.Add(repo.GetQuery().FirstOrDefault(x=>x.Id == permission));
            }

            _uow.PermissionSetRepository.Add(permissionSet);

            _uow.SaveChanges();

            return permissionSet;
        }

        private void InvokeCacheUpdate(Guid dataObjectId, string dataObjectType)
        {
            lock (_cache)
            {
                //update cache with new ObjectRolePermissions
                var rolePermissions = GetRecordBasedPermissionSetForObject(dataObjectId, dataObjectType);
                _cache.AddOrUpdateRecordBasedPermissionSet($"{dataObjectType}:{dataObjectId}", rolePermissions);
            }
        }
    }
}
