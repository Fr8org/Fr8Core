using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;
using System.Web.Http.Dispatcher;
using terminalPapertrail.Tests.Infrastructure;

[assembly: OwinStartup(typeof(terminalPapertrail.Startup))]

namespace terminalPapertrail
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalPapertrailMapBootstrapper.LiveConfiguration);
            WebApiConfig.Register(_configuration);
            app.UseWebApi(_configuration);
            StartHosting("terminalPapertrail");
        }

        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActionController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}