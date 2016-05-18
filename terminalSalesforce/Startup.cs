using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using TerminalBase;
using TerminalBase.BaseClasses;
using terminalSalesforce;
using TerminalBase.Infrastructure;
using System.Web.Http.Dispatcher;
using terminalSalesforce.Actions;
using TerminalBase.Services;

[assembly: OwinStartup(typeof(terminalSalesforce.Startup))]

namespace terminalSalesforce
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalSalesforceStructureMapBootstrapper.LiveConfiguration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalSalesforce");
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
