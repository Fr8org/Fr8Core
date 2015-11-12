using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalGoogle
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BasePluginWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalGoogleServer",
                routeTemplate: "terminalGoogle/{controller}/{id}"
            );
        }
    }
}