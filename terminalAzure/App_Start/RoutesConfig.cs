using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalAzure
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Azure", config);
        }
    }
}
