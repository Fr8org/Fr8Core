using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
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
            Configuration(app, GlobalConfiguration.Configuration);
        }

        public void Configuration(IAppBuilder app, HttpConfiguration configuration, bool selfHost = false)
        {
            if (!selfHost)
            {
                ObjectFactory.Initialize();
                ObjectFactory.Configure(StructureMapBootStrapper.LiveConfiguration);
            }
            ObjectFactory.Configure(PluginAzureSqlServerStructureMapRegistries.LiveConfiguration);

            RoutesConfig.Register(configuration);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            // Configure formatters
            // Enable camelCasing in JSON responses
            var formatters = configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            Task.Run(() =>
            {
                BasePluginController curController = new BasePluginController();
                curController.AfterStartup("plugin_azure_sql_server");
            });
        }
}
}
