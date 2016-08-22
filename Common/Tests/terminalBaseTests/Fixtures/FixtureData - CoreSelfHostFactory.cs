using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Microsoft.Owin.Hosting;
using Owin;

namespace terminalBaseTests.Fixtures
{
    partial class FixtureData
    {
        public static readonly string CoreEndPoint = "http://localhost:30643";

        public class ActivitiesController_ControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new Type[] {
                    typeof(HubWeb.Controllers.PlanNodesController)
                };
            }
        }


        public class ActivitiesController_SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();
                config.Services.Replace(typeof(IHttpControllerSelector), new Hub.Infrastructure.CustomSelector(config));
                // Web API routes
                config.Routes.MapHttpRoute(
                    name: "DefaultApiWithAction",
                    routeTemplate: "api/v1/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
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

                config.Services.Replace(
                    typeof(IHttpControllerTypeResolver),
                    new ActivitiesController_ControllerTypeResolver()
                );
                
                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateCoreServer_ActivitiesController()
        {
            return WebApp.Start<ActivitiesController_SelfHostStartup>(url: CoreEndPoint);
        }
    }
}
