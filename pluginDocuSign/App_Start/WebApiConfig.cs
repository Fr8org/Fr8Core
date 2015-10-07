using System.Web.Http;
using TerminalBase.BaseClasses;

namespace terminal_DocuSign
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            BaseTerminalWebApiConfig.Register(config);

            config.Routes.MapHttpRoute(
                name: "TerminalDocuSign",
                routeTemplate: "terminal_docusign/{controller}/{id}"
            );
        }
    }
}
