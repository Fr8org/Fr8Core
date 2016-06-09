using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalPapertrail.Tests.Infrastructure;
using terminalPapertrail.Actions;

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
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.TerminalController)
                };
        }
        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Write_To_Log_v1>(Write_To_Log_v1.ActivityTemplateDTO);
        }
    }
}