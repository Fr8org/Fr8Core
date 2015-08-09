using System;
using System.Web.Mvc;

namespace KwasantCore.Managers
{
    public class KwasantAuthorizeAttribute : AuthorizeAttribute
    {
        public KwasantAuthorizeAttribute(params string[] roles)
        {
            Roles = String.Join(",", roles);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            // redirect to Error page
            context.Result = new RedirectResult("/Account/InterceptLogin?returnUrl=" + context.RequestContext.HttpContext.Request.RawUrl + "&urlReferrer=" + context.RequestContext.HttpContext.Request.UrlReferrer);
        }
    }
}
