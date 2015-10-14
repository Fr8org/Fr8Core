using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure.StructureMap
{
    public interface ISecurityServices
    {
        void Login(IUnitOfWork uow, DockyardAccountDO dockyardAccountDO);
        // Can throw AuthenticationException
        DockyardAccountDO GetCurrentAccount(IUnitOfWork uow);
        String GetCurrentUser();
        String GetUserName();
        String[] GetRoleNames();
        bool IsCurrentUserHasRole(string role);
        bool IsAuthenticated();
        void Logout();
    }
}