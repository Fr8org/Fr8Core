using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalStatX;
using terminalStatX.Activities;
using terminalStatX.App_Start;
using terminalStatX.Controllers;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalStatX
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
            ConfigureProject(selfHost, TerminalStatXBootstrapper.ConfigureLive);
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting();
            }
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActivityController),
                    typeof(AuthenticationController),
                    typeof(TerminalController)
                };
        }
        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Update_Stat_v1>(Update_Stat_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Stat_Changes_v1>(Monitor_Stat_Changes_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Create_Stat_v1>(Create_Stat_v1.ActivityTemplateDTO);
        }
    }
}


