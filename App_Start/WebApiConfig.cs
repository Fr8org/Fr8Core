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
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name : "DefaultApi",
				routeTemplate : "api/{controller}/{id}",
				defaults : new { id = RouteParameter.Optional }
				);
            HttpConfiguration config1 = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.SerializerSettings.Formatting =
                Newtonsoft.Json.Formatting.Indented;

            //Added XML Serializer - used for Salesforce Outbound Message Response
            config.Formatters.XmlFormatter.UseXmlSerializer = true;

            config.Services.Replace(typeof(IExceptionHandler), new Fr8ExceptionHandler());
		}
	}
}