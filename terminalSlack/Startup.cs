using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalSlack.Actions;
using terminalSlack.Activities;

[assembly: OwinStartup(typeof(terminalSlack.Startup))]

namespace terminalSlack
{
    public class Startup : BaseConfiguration
    {
        public Startup()
            : base(TerminalData.TerminalDTO)
        {
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalSlackBootstrapper.ConfigureLive);
            SwaggerConfig.Register(_configuration);
            WebApiConfig.Register(_configuration);
            app.UseWebApi(_configuration);
            StartHosting();
        }

        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }
        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Monitor_Channel_v1>(Monitor_Channel_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Channel_v2>(Monitor_Channel_v2.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Publish_To_Slack_v1>(Publish_To_Slack_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Publish_To_Slack_v2>(Publish_To_Slack_v2.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController),
                    typeof(Controllers.AuthenticationController)
                };
        }

    }
}
