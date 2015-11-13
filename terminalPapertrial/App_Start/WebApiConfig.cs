using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalPapertrial
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalPapertrial",
                routeTemplate: "terminal_papertrial/{controller}/{id}"
            );
        }
    }
}
