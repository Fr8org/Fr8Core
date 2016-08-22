using System.Web.Mvc;
using System.Web.Routing;

namespace HubWeb.App_Start
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
                name: "Redirect",
                url: "sms",
                defaults: new { controller = "Redirect", action = "TwilioSMS" }
            );

            routes.MapRoute(
                name: "Plan_Directory",
                url: "plan_directory",
                defaults: new { controller = "PlanDirectory", action = "Index" }
            );

            routes.MapRoute(
                name: "Manifest_Registry",
                url: "manifest_registry",
                defaults: new { controller = "ManifestRegistry", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional}
            );
        }
    }
}
