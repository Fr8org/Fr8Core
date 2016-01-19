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
using TerminalBase.Infrastructure;

[assembly: OwinStartup("TerminalExcelConfiguration", typeof(terminalExcel.Startup))]

namespace terminalExcel
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalExcelStructureMapRegistries.LiveConfiguration);
            RoutesConfig.Register(_configuration);

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalExcel");
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
