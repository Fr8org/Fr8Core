using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace $safeprojectname$
{
    public static class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("$safeprojectname$", config);
        }
    }
}