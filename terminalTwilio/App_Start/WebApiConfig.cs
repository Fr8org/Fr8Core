using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalTwilio
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BasePluginWebApiConfig.Register(config);
            config.Routes.MapHttpRoute(
               name: "PluginTwilio",
               routeTemplate: "plugin_twilio/{controller}/{id}"
           );
        }
    }
}
