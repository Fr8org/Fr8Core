using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalBox;
using terminalBox.Actions;

[assembly: OwinStartup(typeof(terminalBox.Startup))]

namespace terminalBox
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
            ConfigureProject(selfHost, BoxStructureMapBootstrapper.LiveConfiguration);

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
            ActivityStore.RegisterActivity<GenerateTableActivity_v1>(GenerateTableActivity_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<SaveToFile_v1>(SaveToFile_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.AuthenticationController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
