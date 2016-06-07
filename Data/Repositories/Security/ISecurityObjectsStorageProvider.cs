using System;
using System.Collections.Generic;
using Data.Repositories.Security.Entities;
using fr8.Infrastructure.Data.DataTransferObjects;

namespace Data.Repositories.Security
{
    public interface ISecurityObjectsStorageProvider
    {
        int InsertRolePermission(RolePermission rolePermission);
        int UpdateRolePermission(RolePermission rolePermission);
        int InsertObjectRolePermission(string currentUserId, string dataObjectId, Guid rolePermissionId, string dataObjectType, string propertyName = null);
        int RemoveObjectRolePermission(string dataObjectId, Guid rolePermissionId, string propertyName = null);
        ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(string dataObjectId);
        List<PermissionDTO> GetAllPermissionsForUser(Guid profileId);
        List<int> GetObjectBasedPermissionSetForObject(string dataObjectId, string dataObjectType, Guid profileId);
        void SetDefaultObjectSecurity(string currentUserId, string dataObjectId, string dataObjectType, Guid rolePermissionId, int? organizationId);
        RolePermission GetRolePermission(string roleName, Guid permissionSetId);
    }
}
