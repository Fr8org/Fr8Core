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
        int InsertObjectRolePrivilege(Guid dataObjectId, Guid rolePrivilegeId, string dataObjectType);
        int RemoveObjectRolePrivilege(Guid dataObjectId, Guid rolePrivilegeId);
        IEnumerable<RolePrivilege> GetRolePrivilegesForSecuredObject(Guid dataObjectId);
        IEnumerable<RolePrivilege> GetRolePrivilegesForFr8Account(Guid fr8AccountId);
        void SetDefaultObjectSecurity(Guid dataObjectId, string dataObjectType);
    }
}
