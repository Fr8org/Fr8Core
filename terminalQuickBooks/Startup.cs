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

[assembly: OwinStartup(typeof(terminalQuickBooks.Startup))]

namespace terminalQuickBooks
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureProject(false, (configuration) => { });

            WebApiConfig.Register(_configuration);
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

            if (!false)
            {
                StartHosting("terminal_quick_books");
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
