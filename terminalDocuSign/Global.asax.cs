using System.Web.Mvc;
using System.Web.Routing;
namespace terminalDocuSign
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "Default",
                url: "environmentSelection",
                defaults: new { controller = "EnvironmentSelection", action = "Index" }
            );
        }
        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
        }
    }
}