using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using HubWeb.ExceptionHandling;
using System.Web.Http.Routing;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using Hub.Infrastructure;

namespace HubWeb
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
            // Web API configuration and services
            config.Services.Replace(typeof(IHttpControllerSelector), new CustomSelector(config));
            // Web API routes
            RegisterAuthenticationEndPoints(config);

            config.Routes.MapHttpRoute(
               name: "DefaultApiWithAction",
               routeTemplate: "api/v1/{controller}/{action}/{id}",
               defaults: new { id = RouteParameter.Optional },
               constraints:new {action = @"(?!^\d+$)^.+$" }
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

            config.Formatters.JsonFormatter.SerializerSettings.Formatting =
                Newtonsoft.Json.Formatting.Indented;

            config.Services.Replace(typeof(IExceptionHandler), new Fr8ExceptionHandler());
		}


        /// <summary>
        /// Configuring specific end-points for AuthenticationToken related APIs.
        /// Sure, this could be done via RouteAttribute (unfortunatelly ActionNameAttribute cannot contain slashes).
        /// However, RA would contain whole "api/v1/...." route, and as we have desided previously we should avoid using RA, and stick more to the actual code.
        /// Yes, we could move auth-token related APIs to separate Controller class,
        /// however "/authenticationtoken" or "/authentication_token" end-points look somewhat ugly.
        /// As mentioned in FR-3383, desicion was made towards "/authentication/tokens".
        /// It makes sense to put all route-configuration stuff in one file, instead of splitting it into small pieces with RA.
        /// </summary>
        private static void RegisterAuthenticationEndPoints(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "AuthenticationTokenRevoke",
                routeTemplate: "api/v1/authentication/tokens/revoke/{id}",
                defaults: new { id = RouteParameter.Optional, controller = "Authentication", action = "RevokeToken" }
            );

            config.Routes.MapHttpRoute(
                name: "AuthenticationTokenGrant",
                routeTemplate: "api/v1/authentication/tokens/grant",
                defaults: new { id = RouteParameter.Optional, controller = "Authentication", action = "GrantTokens" }
            );
	    }
    }
}