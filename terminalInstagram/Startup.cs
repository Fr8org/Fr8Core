using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalInstagram.Actions;
using terminalInstagram.Controllers;
using terminalInstagram;
using terminalInstagram.Models;
using System.Web.Http;
using terminalInstagram.Infrastructure;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalInstagram
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
            ConfigureProject(selfHost, TerminalInstagramBootstrapper.ConfigureLive);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);
            if (!selfHost)
            {
                StartHosting();
            }
            _configuration.BindParameter(typeof(VerificationMessage), new InstagramVerificationModelBinder());
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Monitor_For_New_Media_Posted_v1>(Monitor_For_New_Media_Posted_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActivityController),
                    typeof(TerminalController),
                    typeof(AuthenticationController),
                    typeof(EventController)
                };
        }
    }
}
