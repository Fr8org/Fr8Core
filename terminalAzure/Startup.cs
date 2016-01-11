using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using StructureMap;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using TerminalBase;
using TerminalBase.BaseClasses;

[assembly: OwinStartup(typeof(terminalAzure.Startup))]

namespace terminalAzure
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalAzureSqlServerStructureMapRegistries.LiveConfiguration);
            
            RoutesConfig.Register(_configuration);
            //if (selfHost)
            //{
            //    // Web API routes
            //    configuration.Services.Replace(
            //        typeof(IHttpControllerTypeResolver),
            //        new PluginControllerTypeResolver()
            //    );
            //}

            //DataAutoMapperBootStrapper.ConfigureAutoMapper();

            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalAzure");
            }
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActionController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
