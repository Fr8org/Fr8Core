using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalGoogle
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Google", config);
        }
    }
}