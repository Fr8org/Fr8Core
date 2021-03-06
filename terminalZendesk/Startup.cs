﻿using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Owin;
using System.Web.Http.Dispatcher;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalZendesk.Activities;

[assembly: OwinStartup("ZendeskStartup", typeof(terminalZendesk.Startup))]
namespace terminalZendesk
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
            ConfigureProject(selfHost, TerminalZendeskStructureMapBootstrapper.LiveConfiguration);
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
                    typeof(Controllers.TerminalController)
                };
        }
        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Create_Ticket_v1>(Create_Ticket_v1.ActivityTemplateDTO);
            ActivityStore.RegisterActivity<Monitor_Ticket_v1>(Monitor_Ticket_v1.ActivityTemplateDTO);
        }
    }
}