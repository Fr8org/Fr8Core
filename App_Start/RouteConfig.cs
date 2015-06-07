using System.Web.Mvc;
using System.Web.Routing;

namespace KwasantWeb.App_Start
{
    public class RouteConfig
    {
        public const string ShowNegotiationResponseUrl = "crr";

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ShowNegotiationResponse",
                url: ShowNegotiationResponseUrl,
                defaults: new { controller = "ClarificationRequest", action = "ShowNegotiationResponse", enc = UrlParameter.Optional });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional});
        }
    }
}
