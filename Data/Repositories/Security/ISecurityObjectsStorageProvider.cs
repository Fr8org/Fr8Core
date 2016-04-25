using System;
using System.Collections.Generic;
using Data.Repositories.Security.Entities;
using Data.States;

namespace Data.Repositories.Security
{
    public interface ISecurityObjectsStorageProvider
    {
        int InsertRolePrivilege(RolePrivilege rolePrivilege);
        int UpdateRolePrivilege(RolePrivilege rolePrivilege);
        int InsertObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string dataObjectType, string propertyName = null);
        int RemoveObjectRolePrivilege(string dataObjectId, Guid rolePrivilegeId, string propertyName = null);
        ObjectRolePrivilegesDO GetRolePrivilegesForSecuredObject(string dataObjectId);
        List<RolePrivilege> GetRolePrivilegesForFr8Account(Guid fr8AccountId);
        void SetDefaultObjectSecurity(string dataObjectId, string dataObjectType);
    }
}
