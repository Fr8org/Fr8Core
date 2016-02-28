using System.Web.Http;
using TerminalBase;
using System.Web;
using System.Web.Routing;

namespace TerminalBase.BaseClasses
{
    public static class BaseTerminalWebApiConfig
    {
        public static void Register(string curTerminalName, HttpConfiguration curTerminalConfiguration)
        {
            
            var name = string.Format("terminal_{0}", curTerminalName);
            //map attribute routes
            curTerminalConfiguration.MapHttpAttributeRoutes();

            curTerminalConfiguration.Routes.MapHttpRoute(
                name: name,
                routeTemplate: string.Format("terminal_{0}", curTerminalName) + "/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            curTerminalConfiguration.Routes.MapHttpRoute(
                name: string.Format("Terminal{0}ActivityCatchAll", curTerminalName),
                routeTemplate: "activities/{*actionType}",
                defaults: new { controller = "Activity", action = "Execute", terminal = name }); //It calls ActionController#Execute in an MVC style
            
            //add Web API Exception Filter
            curTerminalConfiguration.Filters.Add(new WebApiExceptionFilterAttribute());
        }
    }
}
