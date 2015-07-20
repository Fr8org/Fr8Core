using System;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace pluginAzureSqlServer
{
    public class SelfHostFactory
    {
        public static HttpSelfHostServer CreateServer(string url)
        {
            var config = new HttpSelfHostConfiguration(url);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
                );

            return new HttpSelfHostServer(config);
        }
    }
}