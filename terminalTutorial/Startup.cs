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
using System.Web.Http.Dispatcher;
using TerminalBase.Services;
using terminalTutorial.Actions;

[assembly: OwinStartup(typeof(terminalTutorial.Startup))]

namespace terminalTutorial
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, null);
            WebApiConfig.Register(_configuration);
            app.UseWebApi(_configuration);
            StartHosting("terminalTutorial");
        }

        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        protected override void RegisterActivities()
        {
           ActivityStore.RegisterActivity<Generate_Simple_Message_v1>(Generate_Simple_Message_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.TerminalController),
                    typeof(Controllers.AuthenticationController)
                };
        }
    }
}
