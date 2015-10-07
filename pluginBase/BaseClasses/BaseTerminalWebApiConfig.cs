using System.Web.Http;
using terminal_base;

namespace TerminalBase.BaseClasses
{
    public static class BaseTerminalWebApiConfig
    {
        public static void Register(HttpConfiguration curTerminalConfiguration)
        {
            //map attribute routes
            curTerminalConfiguration.MapHttpAttributeRoutes();

            //add Web API Exception Filter
            curTerminalConfiguration.Filters.Add(new WebApiExceptionFilterAttribute());
        }
    }
}
