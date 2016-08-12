using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Routing;

namespace HubWeb.Filters
{

    public class RedirecLogedUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var claimsIdentity = (ClaimsIdentity)filterContext.HttpContext.User.Identity;
            var isGuest = claimsIdentity.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == "Guest");

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "dashboard",
                    action = "Index"
                }));
            }
        }
    }
}