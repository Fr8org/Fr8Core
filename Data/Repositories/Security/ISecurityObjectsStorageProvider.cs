using System;
using System.Collections.Generic;
using System.Security;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories.Security.Entities;
using Data.States;
using Fr8Data.DataTransferObjects;

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
        void SetDefaultObjectSecurity(string currentUserId, string dataObjectId, string dataObjectType, Guid rolePermissionId);
        RolePermission GetRolePermission(string roleName, Guid permissionSetId);
    }
}
