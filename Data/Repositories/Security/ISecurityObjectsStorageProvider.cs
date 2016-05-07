using System;
using System.Collections.Generic;
using System.Security;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Repositories.Security.Entities;
using Data.States;

namespace Data.Repositories.Security
{
    public interface ISecurityObjectsStorageProvider
    {
        int InsertRolePermission(RolePermission rolePermission);
        int UpdateRolePermission(RolePermission rolePermission);
        int InsertObjectRolePermission(string dataObjectId, Guid rolePermissionId, string dataObjectType, string propertyName = null);
        int RemoveObjectRolePermission(string dataObjectId, Guid rolePermissionId, string propertyName = null);
        ObjectRolePermissionsWrapper GetRecordBasedPermissionSetForObject(string dataObjectId);
        List<PermissionDTO> GetAllPermissionsForUser(List<string> roleNames);
        List<int> GetObjectBasedPermissionSetForObject(string dataObjectId, string dataObjectType, List<string> roleNames);
        void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType);
    }
}
