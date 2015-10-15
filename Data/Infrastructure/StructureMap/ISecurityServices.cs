using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Data.Infrastructure.StructureMap
{
    public interface ISecurityServices
    {
        void Login(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO);
        String GetCurrentUser();
        String GetUserName();
        String[] GetRoleNames();
        bool IsAuthenticated();
        void Logout();
    }
}