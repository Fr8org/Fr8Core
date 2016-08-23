using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalWord
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("terminalWord", config);
        }
    }
}