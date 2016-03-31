using System;
using System.Collections.Generic;
using Data.Repositories.Security.Entities;

namespace Data.Repositories.Security
{
    public interface ISecurityObjectsStorage
    {
        int InsertRolePrivilege(ISqlConnectionProvider connectionProvider, RolePrivilege rolePrivilege);
        int UpdateRolePrivilege(ISqlConnectionProvider connectionProvider, RolePrivilege rolePrivilege);
        int InsertObjectRolePrivilege(ISqlConnectionProvider connectionProvider, Guid objectId, Guid rolePrivilegeId);
        int RemoveObjectRolePrivilege(ISqlConnectionProvider connectionProvider, Guid objectId, Guid rolePrivilegeId);
        IEnumerable<RolePrivilege> GetRolePrivilegesForSecuredObject(ISqlConnectionProvider connectionProvider,Guid securedObjectId);
        IEnumerable<RolePrivilege> GetRolePrivilegesForFr8Account(ISqlConnectionProvider connectionProvider,Guid fr8AccountId);
        void SetupDefaultSecurityForDataObject(ISqlConnectionProvider connectionProvider, Guid dataObjectId);
    }
}
