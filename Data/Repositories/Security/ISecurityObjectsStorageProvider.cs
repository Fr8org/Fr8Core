using System;
using System.Collections.Generic;
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
        ObjectRolePermissionsDO GetRolePermissionsForSecuredObject(string dataObjectId);
        List<RolePermission> GetRolePermissionsForFr8Account(Guid fr8AccountId);
        void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType);
    }
}
