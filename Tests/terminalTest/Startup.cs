using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using terminalTest.Actions;

[assembly: OwinStartup(typeof(terminalTest.Startup))]
namespace terminalTest
{
    public class Startup: BaseConfiguration
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
            ConfigureProject(selfHost, null);
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
            ActivityStore.RegisterActivity<SimpleActivity_v1>(SimpleActivity_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<SimpleHierarchicalActivity_v1>(SimpleHierarchicalActivity_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<SimpleJumperActivity_v1>(SimpleJumperActivity_v1.ActivityTemplateDTO);
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(Controllers.ActivityController),
                    typeof(Controllers.TerminalController)
                };
        }
    }
}
