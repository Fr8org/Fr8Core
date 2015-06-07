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
        private UserDO _currentLoggedInUser;
        public void Login(IUnitOfWork uow, UserDO userDO)
        {
            lock (_locker)
                _currentLoggedInUser = userDO;
        }

        public String GetCurrentUser()
        {
            lock (_locker)
                return _currentLoggedInUser == null ? String.Empty : _currentLoggedInUser.Id;
        }

        public string GetUserName()
        {
            lock (_locker)
                return _currentLoggedInUser == null ? String.Empty : (_currentLoggedInUser.FirstName + " " + _currentLoggedInUser.LastName);
        }

        public String[] GetRoleNames()
        {
            lock (_locker)
            {
                var roleIds = _currentLoggedInUser == null ? Enumerable.Empty<string>() : _currentLoggedInUser.Roles.Select(r => r.RoleId);
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
                _currentLoggedInUser = null;
        }
    }
}
