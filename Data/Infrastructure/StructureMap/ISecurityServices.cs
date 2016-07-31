﻿using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
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
        bool AuthorizeActivity(PermissionType permissionType, string curObjectId, string curObjectType, string propertyName = null);
        bool UserHasPermission(PermissionType permissionType, string objectType);
        void SetDefaultRecordBasedSecurityForObject(string roleName, string dataObjectId, string dataObjectType);
        IEnumerable<TerminalDO> GetAllowedTerminalsByUser(IEnumerable<TerminalDO> terminals);
    }
}