using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalTelegram;
using terminalTelegram.Activities;
using terminalTelegram.Controllers;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalTelegram
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalTelegramBootstrapper.ConfigureLive);
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);
            if (!selfHost)
            {
                StartHosting();
            }
        }

        public Startup() : base(TerminalData.TerminalDTO)
        {
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<PostToTelegramV1>(PostToTelegramV1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                typeof(ActivityController),
                typeof(AuthenticationController),
                typeof(TerminalController)
            };
        }
    }
}
