using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Data.Infrastructure.StructureMap
{
    public class MockedSecurityServices : ISecurityServices
    {
        private readonly object _locker = new object();
        private DockyardAccountDO _currentLoggedInDockyardAccount;

        public void Login(IUnitOfWork uow, DockyardAccountDO dockyardAccountDO)
        {
            lock (_locker)
                _currentLoggedInDockyardAccount = dockyardAccountDO;
        }

        public DockyardAccountDO GetCurrentAccount(IUnitOfWork uow)
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
    }
}
