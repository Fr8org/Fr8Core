using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;
using terminalDocuSign.Controllers;

namespace terminalDocuSign
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
              name: "DocuSignEnvironmentSelection",
              routeTemplate: "environmentSelection",
              defaults: new { controller = "EnvironmentSelection", action = "Index", terminal = "terminal_DocuSign" }
              );
            BaseTerminalWebApiConfig.Register("DocuSign", config);
        }
    }
}
