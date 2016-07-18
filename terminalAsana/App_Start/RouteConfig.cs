using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalAsana
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("terminalAsana", config);
        }
    }
}