using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Microsoft.Owin;
using Owin;
using terminalAzure.Activities;
using TerminalBase.BaseClasses;
using Utilities.Configuration.Azure;

[assembly: OwinStartup(typeof(terminalAzure.Startup))]

namespace terminalAzure
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            ConfigureProject(selfHost, TerminalAzureSqlServerStructureMapRegistries.LiveConfiguration);
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();
            app.UseWebApi(_configuration);
            if (!selfHost)
            {
                StartHosting("terminalAzure");
            }
        }

        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Write_To_Sql_Server_v1>(Write_To_Sql_Server_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.EventController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
