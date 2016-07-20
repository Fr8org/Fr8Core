using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Owin;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using terminalSalesforce.Actions;

[assembly: OwinStartup(typeof(terminalSalesforce.Startup))]

namespace terminalSalesforce
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
            ConfigureProject(selfHost, TerminalSalesforceStructureMapBootstrapper.LiveConfiguration);
            SwaggerConfig.Register(_configuration);
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
            ActivityStore.RegisterActivity<Save_To_SalesforceDotCom_v1>(Save_To_SalesforceDotCom_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Get_Data_v1>(Get_Data_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Post_To_Chatter_v1>(Post_To_Chatter_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Post_To_Chatter_v2>(Post_To_Chatter_v2.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Mail_Merge_From_Salesforce_v1>(Mail_Merge_From_Salesforce_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Salesforce_Event_v1>(Monitor_Salesforce_Event_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.AuthenticationController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
