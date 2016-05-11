using System.Security.Claims;
using System.Web.Mvc;

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
    }
}