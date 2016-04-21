using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Security;
using Data.States;
using StructureMap;

namespace Data.Infrastructure.StructureMap
{
    public class MockedSecurityServices : ISecurityServices
    {
        private readonly object _locker = new object();
        private Fr8AccountDO _currentLoggedInDockyardAccount;
        public void Login(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO)
        {
            lock (_locker)
                _currentLoggedInDockyardAccount = dockyardAccountDO;
        }
        public Fr8AccountDO GetCurrentAccount(IUnitOfWork uow)
        {
            lock (_locker)
            {
                return _currentLoggedInDockyardAccount;
            }
        }

        public String GetCurrentUser()
        {
            lock (_locker)
                return _currentLoggedInDockyardAccount == null ? String.Empty : _currentLoggedInDockyardAccount.Id;
        }

        public string GetUserName()
        {
            lock (_locker)
                return _currentLoggedInDockyardAccount == null ? String.Empty : (_currentLoggedInDockyardAccount.FirstName + " " + _currentLoggedInDockyardAccount.LastName);
        }

        public bool IsCurrentUserHasRole(string role)
        {
            return GetRoleNames().Any(x => x == role);
        }

        public String[] GetRoleNames()
        {
            lock (_locker)
            {
                var roleIds = _currentLoggedInDockyardAccount == null ? Enumerable.Empty<string>() : _currentLoggedInDockyardAccount.Roles.Select(r => r.RoleId);
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    return roleIds.Select(id => uow.AspNetRolesRepository.GetByKey(id).Name).ToArray();
                }
            }
        }

        public bool IsAuthenticated()
        {
            lock (_locker)
                return !String.IsNullOrEmpty(GetCurrentUser());
        }

        public void Logout()
        {
            lock (_locker)
                _currentLoggedInDockyardAccount = null;
        }

        public ClaimsIdentity GetIdentity(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultObjectSecurity(Guid dataObjectId, string dataObjectType)
        {
            var securityStorageProvider = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
            securityStorageProvider.SetDefaultObjectSecurity(dataObjectId.ToString(), dataObjectType);
        }

        public bool AuthorizeActivity(Permission permissionName, string curObjectId, string propertyName = null)
        {
            //get all current roles for current user
            var roles = GetRoleNames().ToList();

            //get all role permissions for object
            var securityStorageProvider = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
            var objRolePermissionWrapper = securityStorageProvider.GetRolePermissionsForSecuredObject(curObjectId);

            if (objRolePermissionWrapper == null)
                return false;

            if (string.IsNullOrEmpty(propertyName))
            {
                var authorizedRoles = objRolePermissionWrapper.RolePermissions.Where(x => roles.Contains(x.Role.RoleName));
                return authorizedRoles.Any();
            }
            else
            {
                //find property inside object properties collection with permissions
                if (!objRolePermissionWrapper.Properties.ContainsKey(propertyName)) return false;

                var propertyRolePermissions = objRolePermissionWrapper.Properties[propertyName];
                var authorizedRoles = propertyRolePermissions.Where(x => roles.Contains(x.Role.RoleName));
                return authorizedRoles.Any();
            }
        }
    }
}
