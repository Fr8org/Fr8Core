using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
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
                    typeof(HubWeb.Controllers.PlanNodesController),
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
                var payloadDTO = new PayloadDTO(Fr8.Testing.Unit.Fixtures.FixtureData.TestContainer_Id_1());
                using (var crateStorage = new CrateManager().GetUpdatableStorage(payloadDTO))
                {
                    var operationalStatus = new OperationalStateCM();
                    var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                    crateStorage.Add(operationsCrate);
                }
                return payloadDTO;
            }
        }
    }
}
