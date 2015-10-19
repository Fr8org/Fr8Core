using System.Web.Http;
using PluginBase;

namespace PluginBase.BaseClasses
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
