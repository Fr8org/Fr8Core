using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Core.StructureMap;
using Data.Infrastructure.AutoMapper;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using PluginBase;
using PluginBase.BaseClasses;
using StructureMap;

[assembly: OwinStartup(typeof(pluginAzureSqlServer.Startup))]

namespace pluginAzureSqlServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            HttpConfiguration configuration = new HttpConfiguration();

            if (!selfHost)
            {
                ObjectFactory.Initialize();
                ObjectFactory.Configure(StructureMapBootStrapper.LiveConfiguration);
            }
            ObjectFactory.Configure(PluginAzureSqlServerStructureMapRegistries.LiveConfiguration);

            RoutesConfig.Register(configuration);
            if (selfHost)
            {
                // Web API routes
                configuration.Services.Replace(
                    typeof(IHttpControllerTypeResolver),
                    new PluginControllerTypeResolver()
                );
            }

            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            // Configure formatters
            // Enable camelCasing in JSON responses
            var formatters = configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            app.UseWebApi(configuration);

            if (!selfHost)
            {
                Task.Run(() =>
                {
                    BasePluginController curController = new BasePluginController();
                    curController.AfterStartup("plugin_azure_sql_server");
                });
            }
        }

        public class PluginControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new Type[] {
                    typeof(Controllers.ActionController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.PluginController)
                };
            }
        }

    }
}
