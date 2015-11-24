using System.Web.Http;
using TerminalBase;
using System.Web;

namespace TerminalBase.BaseClasses
{
    public static class BaseTerminalWebApiConfig
    {
        public static void Register(HttpConfiguration curTerminalConfiguration)
        {
            //map attribute routes
            curTerminalConfiguration.MapHttpAttributeRoutes();

            curTerminalConfiguration.Routes.MapHttpRoute(
                   name: "TerminalBase",
                   routeTemplate: "{terminal}/{controller}/{id}",
                   defaults: new { id = RouteParameter.Optional }
               );
            curTerminalConfiguration.Routes.MapHttpRoute(
                  name: "TerminalBaseActionCatchAll",
                  routeTemplate: "actions/{*actionType}",
                  defaults: new { controller = "Action", action = "Execute" }); //It calls ActionController#Execute in an MVC style

            //add Web API Exception Filter
            curTerminalConfiguration.Filters.Add(new WebApiExceptionFilterAttribute());
        }
    }
}
