using System.Web.Mvc;
using System.Web.Routing;

namespace Web.App_Start
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
                defaults: new { controller = "ClarificationRequest", action = "ShowNegotiationResponse", enc = UrlParameter.Optional }
                );

            routes.MapRoute(
                name: "AngularTemplates",
                url: "AngularTemplate/{template}",
                defaults: new { controller = "AngularTemplate", action = "Markup" }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}
