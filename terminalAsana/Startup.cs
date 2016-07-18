using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalAsana.Controllers;
using terminalAsana;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalAsana
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
            ConfigureProject(selfHost,TerminalAsanaBootstrapper.ConfigureLive);

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
            ActivityStore.RegisterActivity<Activities.Post_Comment_v1>(Activities.Post_Comment_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Activities.Get_Tasks_v1>(Activities.Get_Tasks_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActivityController),
                    typeof(TerminalController),
                    typeof(AuthenticationController)
                };
        }
    }
}
