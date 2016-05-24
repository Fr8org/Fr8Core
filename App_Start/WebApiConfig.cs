using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using HubWeb.ExceptionHandling;
using System.Web.Http.Routing;
using System.Net.Http;

namespace HubWeb
{
	public static class WebApiConfig
	{
		public static void Register( HttpConfiguration config )
		{
			// Web API configuration and services

			// Web API routes

            // NOTE :: API plan is changed for ProcessEvents.  
            config.Routes.MapHttpRoute(
                name: "DefaultApiEvents",
                routeTemplate: "api/v1/events",
                defaults: new { action = "Post", controller = "Events" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) }
                );

            config.Routes.MapHttpRoute(
				name : "DefaultApiWithAction",
				routeTemplate : "api/v1/{controller}/{action}/{id}",
				defaults : new { id = RouteParameter.Optional }
				);
            config.Routes.MapHttpRoute(
                name: "DefaultApiGet",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional, action = "Get" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
                );
            config.Routes.MapHttpRoute(
                name: "DefaultApiPost",
                routeTemplate: "api/v1/{controller}",
                defaults: new { action = "Post" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) }
                );
            config.Routes.MapHttpRoute(
                name: "DefaultApiPut",
                routeTemplate: "api/v1/{controller}",
                defaults: new { action = "Put" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) }
                );
            config.Routes.MapHttpRoute(
                name: "DefaultApiDelete",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional, action = "Delete" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Delete) }
                );            

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/v1/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //    );
            HttpConfiguration config1 = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.SerializerSettings.Formatting =
                Newtonsoft.Json.Formatting.Indented;

            config.Services.Replace(typeof(IExceptionHandler), new Fr8ExceptionHandler());
		}
	}
}