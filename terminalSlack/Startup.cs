using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using TerminalBase.Services;
using terminalSlack.Actions;

[assembly: OwinStartup(typeof(terminalSlack.Startup))]

namespace terminalSlack
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalSlackBootstrapper.ConfigureLive);
            WebApiConfig.Register(_configuration);
            app.UseWebApi(_configuration);
            StartHosting("terminalSlack");
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
