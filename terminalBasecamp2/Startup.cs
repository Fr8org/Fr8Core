using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalBasecamp;
using terminalBasecamp.Activities;
using terminalBasecamp.App_Start;
using terminalBasecamp.Controllers;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalBasecamp
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
