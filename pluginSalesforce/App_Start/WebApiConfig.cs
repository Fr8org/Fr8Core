using System.Web.Http;

namespace terminal_Salesforce
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "TerminalSalesforce",
                routeTemplate: "terminal_salesforce/{controller}/{id}"
            );         
        }
    }
}
