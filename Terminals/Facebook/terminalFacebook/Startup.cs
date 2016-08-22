using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalFacebook;
using terminalFacebook.Activities;
using terminalFacebook.Controllers;
using terminalFacebook.Infrastructure;
using terminalFacebook.Models;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalFacebook
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
            ConfigureProject(selfHost, TerminalFacebookBootstrapper.ConfigureLive);
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);
            if (!selfHost)
            {
                StartHosting();
            }
            _configuration.BindParameter(typeof(VerificationMessage), new FacebookVerificationModelBinder());
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Post_To_Timeline_v1>(Post_To_Timeline_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Feed_Posts_v1>(Monitor_Feed_Posts_v1.ActivityTemplateDTO);
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
