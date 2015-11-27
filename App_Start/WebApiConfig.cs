using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using HubWeb.ExceptionHandling;

namespace HubWeb
{
	public static class WebApiConfig
	{
		public static void Register( HttpConfiguration config )
		{
			// Web API configuration and services

			// Web API routes

			config.Routes.MapHttpRoute(
				name : "DefaultWithActionApi",
				routeTemplate : "api/v1/{controller}/{action}/{id}",
				defaults : new { id = RouteParameter.Optional }
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