using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Hub.Services;
using HubWeb;
using HubWeb.Controllers;
using HubWeb.Controllers.Api;
using Microsoft.Owin.Hosting;
using Owin;

namespace WebHub
{
    public class SelfHostFactory
    {
        public class SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var startup = new Startup();
                startup.Configuration(app);
            }
        }
        public static IDisposable CreateServer(string url)
        {
            return WebApp.Start<SelfHostStartup>(url: url);
        }
        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return new Type[] {
                    typeof(ActionListController),
                    typeof(ActionsController),
                    typeof(AlarmsController),
                    typeof(AuthenticationController),
                    typeof(ConfigurationController),
                    typeof(ContainersController),
                    typeof(CriteriaController),
                    typeof(EventController),
                    typeof(FieldController),
                    typeof(FilesController),
                    typeof(ManageAuthTokenController),
                    typeof(ManifestsController),
                    typeof(ReportController),
                    typeof(RouteNodesController),
                    typeof(RoutesController),
                    typeof(ProcessNodeTemplateController),
                    typeof(TerminalsController),
                    typeof(UserController),
                    typeof(WebServicesController)
                };
        }
    }
}
