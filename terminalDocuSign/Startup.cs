using System;
using System.Collections.Generic;
using System.Web.Http.Dispatcher;
using Microsoft.Owin;
using Owin;
using terminalDocuSign;
using terminalDocuSign.Controllers;
using terminalDocuSign.Infrastructure.AutoMapper;
using TerminalBase.BaseClasses;
using StructureMap;
using System.Data.Entity;
using Hangfire;
using Hub.Security;

[assembly: OwinStartup(typeof(Startup))]

namespace terminalDocuSign
{
    public class Startup : BaseConfiguration
    {
        public void Configuration(IAppBuilder app)
        {
            Configuration(app, false);
        }

        public void Configuration(IAppBuilder app, bool selfHost)
        {
            //ObjectFactory.GetInstance<DbContext>().Database.Initialize(true);

            ConfigureProject(selfHost, TerminalDocusignStructureMapBootstrapper.LiveConfiguration);
            TerminalDataAutoMapperBootStrapper.ConfigureAutoMapper();
            RoutesConfig.Register(_configuration);
            ConfigureFormatters();

            app.UseWebApi(_configuration);

            if (!selfHost)
            {
                StartHosting("terminalDocuSign");
            }

            ConfigureHangfire(app, "DockyardDB");
        }

        public void ConfigureHangfire(IAppBuilder app, string connectionString)
        {
            var options = new BackgroundJobServerOptions
            {
                Queues = new[] { "terminal_docusign" },
            };

            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AuthorizationFilters = new[] { new HangFireAuthorizationFilter() },
            });
            app.UseHangfireServer(options);
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
        }

        public override ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActivityController),
                    typeof(EventController),
                    typeof(TerminalController),
                    typeof(AuthenticationController)
                };
        }
    }
}
