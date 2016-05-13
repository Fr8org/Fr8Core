using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using System.Net.Http;

namespace PlanDirectory.App_Start
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
            config.Routes.MapHttpRoute(
				name : "DefaultApiWithAction",
				routeTemplate : "api/{controller}/{action}/{id}",
				defaults : new { id = RouteParameter.Optional }
			);

            config.Routes.MapHttpRoute(
                name: "DefaultApiGet",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional, action = "Get" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiPost",
                routeTemplate: "api/{controller}",
                defaults: new { action = "Post" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiPut",
                routeTemplate: "api/{controller}",
                defaults: new { action = "Put" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiDelete",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional, action = "Delete" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Delete) }
            );

            HttpConfiguration config1 = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.SerializerSettings.Formatting =
                Newtonsoft.Json.Formatting.Indented;
		}
	}
}