using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using System.Net.Http;
using System.Security.Claims;
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
        void SetDefaultObjectSecurity(Guid dataObjectId, string dataObjectType);
        bool AuthorizeActivity(Privilege privilegeName, string curObjectId, string propertyName = null);
    }
}