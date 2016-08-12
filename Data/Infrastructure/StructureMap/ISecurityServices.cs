using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.States;

namespace Data.Infrastructure.StructureMap
{
    public interface ISecurityServices
    {
        void Login(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO);
        // Can throw AuthenticationException
        Fr8AccountDO GetCurrentAccount(IUnitOfWork uow);
        String GetCurrentUser();
        String GetUserName();
        String[] GetRoleNames();
        bool IsCurrentUserHasRole(string role);
        bool IsAuthenticated();
        void Logout();
        ClaimsIdentity GetIdentity(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO);
        Task<ClaimsIdentity> GetIdentityAsync(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO);
        bool AuthorizeActivity(PermissionType permissionType, Guid curObjectId, string curObjectType, string propertyName = null);
        bool UserHasPermission(PermissionType permissionType, string objectType);
        void SetDefaultRecordBasedSecurityForObject(string roleName, Guid dataObjectId, string dataObjectType, List<PermissionType> customPermissions = null);
        IEnumerable<TerminalDO> GetAllowedTerminalsByUser(IEnumerable<TerminalDO> terminals, bool byOwnershipOnly = false);
        List<string> GetAllowedUserRolesForSecuredObject(Guid objectId, string objectType);
    }
}