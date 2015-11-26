using System.Web.Http;
using TerminalBase.BaseClasses;

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
