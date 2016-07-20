using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalDemo
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Fr8Demo", config);
        }
    }
}