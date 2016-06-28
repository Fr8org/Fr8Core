using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Microsoft.Owin;
using Owin;
using terminalMailChimp;
using terminalMailChimp.Activities;

[assembly: OwinStartup(typeof(Startup))]
namespace terminalMailChimp
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
            ConfigureProject(selfHost, TerminalMailChimpBootstrapper.ConfigureLive);
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
            ActivityStore.RegisterActivity<Update_List_v1>(Update_List_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Send_Using_MailChimp_Template_v1>(Send_Using_MailChimp_Template_v1.ActivityTemplateDTO);
        }
    }
}