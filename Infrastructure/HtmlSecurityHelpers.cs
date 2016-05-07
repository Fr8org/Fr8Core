using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.States;
using StructureMap;

namespace HubWeb.Infrastructure
{
    public static class HtmlSecurityHelpers
    {
        public static bool HasManageUsersPermission(this HtmlHelper helper, PermissionType permissionType)
        {
            var securityServices = ObjectFactory.GetInstance<ISecurityServices>();

            if (!securityServices.IsAuthenticated())
                return false;

            var permissions = securityServices.GetCurrentUserPermissions();
            return permissions.Any(x=>x.Permission == (int)permissionType && x.ObjectType == nameof(Fr8AccountDO));
        }
    }
}