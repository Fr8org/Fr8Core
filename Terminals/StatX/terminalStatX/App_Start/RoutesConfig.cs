using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalStatX.App_Start
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("StatX", config);
        }
    }
}