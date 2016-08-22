using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using HubWeb.Controllers;
using HubWeb.Controllers.Api;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace HubWeb
{
    public class SelfHostFactory
    {
        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                Fr8.Infrastructure.Utilities.Server.ServerPhysicalPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                HttpConfiguration config = new HttpConfiguration();
                WebApiConfig.Register(config);
                app.SetDataProtectionProvider(new DpapiDataProtectionProvider());
                var startup = new Startup();
                startup.Configuration(app, true);
                app.UseWebApi(config);
                ConfigureFormatters(config);
                new MvcApplication().Init(true);
                startup.ConfigureControllerActivator(config);
            }

            private void ConfigureFormatters(HttpConfiguration config)
            {
                // Configure formatters
                // Enable camelCasing in JSON responses
                var formatters = config.Formatters;
                var jsonFormatter = formatters.JsonFormatter;
                var settings = jsonFormatter.SerializerSettings;
                settings.Formatting = Formatting.Indented;
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }
        }
        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostStartup>(url: url);
        }
        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActivitiesController),
                    typeof(AlarmsController),
                    typeof(AuthenticationController),
                    typeof(ConfigurationController),
                    typeof(ContainersController),
                    typeof(DocumentationController),
                    typeof(EventsController),
                    typeof(FilesController),
                    typeof(ManifestsController),
                    typeof(ReportsController),
                    typeof(PlanNodesController),
                    typeof(PlansController),
                    typeof(TerminalsController),
                    typeof(UsersController),
                    typeof(WarehousesController),
                    typeof(WebServicesController)
                };
        }
    }
  
}
