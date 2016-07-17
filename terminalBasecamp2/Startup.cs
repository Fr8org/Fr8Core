using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalBasecamp2;
using terminalBasecamp2.Activities;
using terminalBasecamp2.Controllers;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalBasecamp2
{
    public class Startup : BaseConfiguration
    {
        public Startup()
            : base(TerminalData.TerminalDTO)
        {
        }

        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, StructureMapBootstrapper.LiveMode);
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);
            if (!selfHost)
            {
                StartHosting();
            }
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Create_Message_v1>(Create_Message_v1.ActivityTemplate);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new[]
            {
                    typeof(ActivityController),
                    typeof(TerminalController)
            };
        }
    }
}
