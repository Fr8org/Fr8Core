using System.Web.Http;
using Fr8.TerminalBase.BaseClasses;

namespace terminalPapertrail
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Papertrail", config);
        }
    }
}
