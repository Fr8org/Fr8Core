using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalDemo;
using terminalDemo.Activities;
using terminalDemo.Controllers;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalDemo
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
            ConfigureProject(selfHost, null);
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
            ActivityStore.RegisterActivity<DoMath_v1>(DoMath_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActivityController),
                    typeof(TerminalController),
                };
        }
    }
}
