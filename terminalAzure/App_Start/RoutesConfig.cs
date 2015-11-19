using System.Web.Http;
using Data.States.Templates;
using TerminalBase.BaseClasses;

namespace terminalAzure
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
           
          BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalAzureSqlServer",
                routeTemplate: "terminal_azure_sql_server/{controller}/{id}");

            config.Routes.MapHttpRoute(
                name: "TerminalAzureActionCatchAll",
                routeTemplate: "actions/{*actionType}",
                defaults: new {controller="Action", action="Execute"});
        }
    }
}
