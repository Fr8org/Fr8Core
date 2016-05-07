using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
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

        public bool AuthorizeActivity(PermissionType permissionName, string curObjectId, string curObjectType, string propertyName = null)
        {
            //check if user is authenticated. Unauthenticated users cannot pass security and come up to here, which means this is internal fr8 event, that need to be passed 
            if (!IsAuthenticated())
                return true;

            //get all current roles for current user
            var roles = GetRoleNames().ToList();
            if (!roles.Any())
                return true;

            //Object Based permission set checks

            var securityStorageProvider = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
            var permissionSets = securityStorageProvider.GetObjectBasedPermissionSetForObject(curObjectId, curObjectType, roles);

            var modifyAllData = permissionSets.FirstOrDefault(x => x == (int) PermissionType.ModifyAllObjects);
            var viewAllData = permissionSets.FirstOrDefault(x => x == (int) PermissionType.ViewAllObjects);

            if (viewAllData != 0 && permissionName == PermissionType.ReadObject) return true;
            if (modifyAllData != 0) return true;

            var currentPermission = permissionSets.FirstOrDefault(x => x == (int) permissionName);
            if (currentPermission != 0) return true;

            return false;
        }

        public List<PermissionDTO> GetCurrentUserPermissions()
        {
            throw new NotImplementedException();
        }
    }
}
