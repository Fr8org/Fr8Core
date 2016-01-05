using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Microsoft.Owin.Hosting;
using Owin;

namespace terminalIntegrationTests.Fixtures
{
    partial class FixtureData
    {
        public static readonly string CoreEndPoint = "http://localhost:30643";

        public class ActivitiesController_ControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new Type[] {
                    typeof(HubWeb.Controllers.RouteNodesController),
                    typeof(HubWeb.Controllers.ContainersController)
                };
            }
        }

        public class ActivitiesController_SelfHostStartup
        {
            public void Configuration(IAppBuilder app)
            {
                var config = new HttpConfiguration();

                // Web API routes
                config.MapHttpAttributeRoutes();

                config.Services.Replace(
                    typeof(IHttpControllerTypeResolver),
                    new ActivitiesController_ControllerTypeResolver()
                );

                app.UseWebApi(config);
            }
        }

        public static IDisposable CreateCoreServer_ActivitiesController()
        {
            return WebApp.Start<ActivitiesController_SelfHostStartup>(url: CoreEndPoint);
        }

        

        public static PayloadDTO CratePayloadDTOForSendEmailViaSendGridConfiguration
        {
            get
            {
                var payloadDTO = new PayloadDTO(UtilitiesTesting.Fixtures.FixtureData.TestContainer_Id_1());
                using (var updater = new CrateManager().UpdateStorage(payloadDTO))
                {
                    var operationalStatus = new OperationalStateCM();
                    var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                    updater.CrateStorage.Add(operationsCrate);
                }
                return payloadDTO;
            }
        }
    }
}
