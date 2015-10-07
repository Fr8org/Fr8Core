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
using terminal_base;
using terminal_base.BaseClasses;
using StructureMap;

[assembly: OwinStartup(typeof(terminal_AzureSqlServer.Startup))]

namespace terminal_AzureSqlServer
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
            ObjectFactory.Configure(TerminalAzureSqlServerStructureMapRegistries.LiveConfiguration);

            RoutesConfig.Register(configuration);
            if (selfHost)
            {
                // Web API routes
                configuration.Services.Replace(
                    typeof(IHttpControllerTypeResolver),
                    new TerminalControllerTypeResolver()
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
                    BaseTerminalController curController = new BaseTerminalController();
                    curController.AfterStartup("terminal_azure_sql_server");
                });
            }
        }

        public class TerminalControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new Type[] {
                    typeof(Controllers.ActionController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
                };
            }
        }

    }
}
