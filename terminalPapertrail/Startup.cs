using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Owin;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using terminalPapertrail.Tests.Infrastructure;
using terminalPapertrail.Actions;

[assembly: OwinStartup(typeof(terminalPapertrail.Startup))]

namespace terminalPapertrail
{
    public class Startup : BaseConfiguration
    {
        public Startup()
            : base(TerminalData.TerminalDTO)
        {
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalPapertrailMapBootstrapper.LiveConfiguration);
            SwaggerConfig.Register(_configuration);
            WebApiConfig.Register(_configuration);
            app.UseWebApi(_configuration);
            StartHosting();
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