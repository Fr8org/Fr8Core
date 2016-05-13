using System.Web.Mvc;
using System.Web.Routing;

namespace PlanDirectory.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Reauthenticate",
                url: "Reauthenticate",
                defaults: new { controller = "Home", action = "Reauthenticate" }
            );

            routes.MapRoute(
                name: "AuthenticateByToken",
                url: "AuthenticateByToken",
                defaults: new { controller = "Home", action = "AuthenticateByToken" }
            );

            routes.MapRoute(
                name: "AngularTemplates",
                url: "AngularTemplate/{template}",
                defaults: new { controller = "AngularTemplate", action = "Markup" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}