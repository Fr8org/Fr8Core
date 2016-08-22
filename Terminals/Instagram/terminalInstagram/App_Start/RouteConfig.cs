using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalInstagram
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Fr8Instagram", config);
        }
    }
}