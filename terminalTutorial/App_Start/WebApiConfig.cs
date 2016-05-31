using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminalTutorial
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register("Tutorial", config);
        }
    }
}
