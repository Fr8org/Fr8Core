using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalGoogle
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalGoogleServer",
                routeTemplate: "terminalGoogle/{controller}/{id}"
            );
        }
    }
}