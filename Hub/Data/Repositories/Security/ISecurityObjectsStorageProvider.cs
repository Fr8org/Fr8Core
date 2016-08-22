using System;
using System.Collections.Generic;
using Data.Repositories.Security.Entities;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Data.Repositories.Security
{
    public interface ISecurityObjectsStorageProvider
    {
        int InsertRolePermission(RolePermission rolePermission);
        int UpdateRolePermission(RolePermission rolePermission);
        int InsertObjectRolePermission(string currentUserId, Guid dataObjectId, Guid rolePermissionId, string dataObjectType, string propertyName = null);
        int RemoveObjectRolePermission(Guid dataObjectId, Guid rolePermissionId, string propertyName = null);
        ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(Guid dataObjectId, string dataObjectType);
        List<PermissionDTO> GetAllPermissionsForUser(Guid profileId);
        List<int> GetObjectBasedPermissionSetForObject(Guid dataObjectId, string dataObjectType, Guid profileId);
        void SetDefaultRecordBasedSecurityForObject(string currentUserId, string roleName, Guid dataObjectId, string dataObjectType, Guid rolePermissionId, int? organizationId, List<PermissionType> customPermissionTypes = null);
        RolePermission GetRolePermission(string roleName, Guid permissionSetId);
        List<string> GetAllowedUserRolesForSecuredObject(Guid objectId, string objectType);
    }
}
