using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using Microsoft.Owin;
using Owin;
using terminalExcel.Actions;
using terminalExcel.Activities;

[assembly: OwinStartup("TerminalExcelConfiguration", typeof(terminalExcel.Startup))]

namespace terminalExcel
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
            ConfigureProject(selfHost, TerminalExcelStructureMapRegistries.LiveConfiguration);
            SwaggerConfig.Register(_configuration);
            RoutesConfig.Register(_configuration);

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
                    typeof(Controllers.TerminalController)
                };
        }
        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Load_Excel_File_v1>(Load_Excel_File_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Save_To_Excel_v1>(Save_To_Excel_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Set_Excel_Template_v1>(Set_Excel_Template_v1.ActivityTemplateDTO);
        }
    }
}
