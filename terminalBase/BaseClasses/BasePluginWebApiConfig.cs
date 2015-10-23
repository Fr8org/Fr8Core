using System.Web.Http;
using TerminalBase;

namespace TerminalBase.BaseClasses
{
    public static class BasePluginWebApiConfig
    {
        public static void Register(HttpConfiguration curPluginConfiguration)
        {
            //map attribute routes
            curPluginConfiguration.MapHttpAttributeRoutes();

            //add Web API Exception Filter
            curPluginConfiguration.Filters.Add(new WebApiExceptionFilterAttribute());
        }
    }
}
