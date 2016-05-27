using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using System.Web.Http.Dispatcher;
using TerminalBase.Services;
using terminalDropbox.Actions;

[assembly: OwinStartup(typeof(terminalDropbox.Startup))]

namespace terminalDropbox
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalDropboxStructureMapBootstrapper.LiveConfiguration);

            RoutesConfig.Register(_configuration);

            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalAzure");
            }
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.AuthenticationController),
                    typeof(Controllers.TerminalController)
                };
        }
        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Get_File_List_v1>(Get_File_List_v1.ActivityTemplateDTO);
        }

    }
}
