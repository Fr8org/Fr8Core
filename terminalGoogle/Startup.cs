using Microsoft.Owin;
using Owin;
using System;
using System.Web.Http.Dispatcher;
using System.Collections.Generic;
using Fr8.TerminalBase.BaseClasses;
using terminalGoogle.Actions;
using terminalGoogle.Activities;

[assembly: OwinStartup(typeof(terminalGoogle.Startup))]

namespace terminalGoogle
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
            ConfigureProject(selfHost, TerminalGoogleBootstrapper.ConfigureLive);
            Container.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting();
            }
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.AuthenticationController),
                    typeof(Controllers.TerminalController)
                };
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Get_Google_Sheet_Data_v1>(Get_Google_Sheet_Data_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Form_Responses_v1>(Monitor_Form_Responses_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Save_To_Google_Sheet_v1>(Save_To_Google_Sheet_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Gmail_Inbox_v1>(Monitor_Gmail_Inbox_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Google_Spreadsheet_Changes_v1>(Monitor_Google_Spreadsheet_Changes_v1.ActivityTemplateDTO);
        }
    }
}
