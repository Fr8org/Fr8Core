using Microsoft.Owin;
using Owin;
using terminalTwilio;
using TerminalBase.BaseClasses;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using System;
using terminalTwilio.Activities;
using TerminalBase.Services;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalTwilio
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalTwilioMapBootstrapper.LiveConfiguration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalTwilio");
            }
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Send_Via_Twilio_v1>(Send_Via_Twilio_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
