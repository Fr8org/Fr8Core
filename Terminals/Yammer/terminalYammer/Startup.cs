using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalYammer.Actions;

[assembly: OwinStartup(typeof(terminalYammer.Startup))]

namespace terminalYammer
{
    public class Startup : BaseConfiguration
    {
        public Startup()
            : base(TerminalData.TerminalDTO)
        {
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, null);
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
            ActivityStore.RegisterActivity<Post_To_Yammer_v1>(Post_To_Yammer_v1.ActivityTemplateDTO);
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
