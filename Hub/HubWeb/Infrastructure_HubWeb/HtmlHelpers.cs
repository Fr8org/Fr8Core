using System.Security.Claims;
using System.Web.Mvc;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.States;
using StructureMap;

namespace HubWeb.Infrastructure_HubWeb
{
    public static class HtmlHelpers
    {
        public static bool IsDebug(this HtmlHelper htmlHelper)
        {
            #if DEV || RELEASE
                    return false;
            #else
                  return true;
            #endif
        }

        public static bool HasUserClaim(this HtmlHelper htmlHelper, string claimType)
        {
            var claimIdentity = htmlHelper.ViewContext.RequestContext.HttpContext.User.Identity as ClaimsIdentity;

            if (claimIdentity == null) return false;

            var claim = claimIdentity.FindFirst(claimType);
            if (claim != null)
                return true;

            return false;
        }

        /// <summary>
        /// Check if current user has Manage Fr8 Users/Manage Internal Users 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="permissionType"></param>
        /// <returns></returns>
        public static bool HasManageUsersPermission(this HtmlHelper helper, PermissionType permissionType)
        {
            var securityServices = ObjectFactory.GetInstance<ISecurityServices>();
            return securityServices.UserHasPermission(permissionType, nameof(Fr8AccountDO));
        }
    }
}