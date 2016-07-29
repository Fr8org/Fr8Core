using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using Segment;
using StructureMap;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Services;
using PlanDirectory.Infrastructure;
using PlanDirectory.Interfaces;

namespace PlanDirectory
{
    public class Global : System.Web.HttpApplication
    {
        protected async void Application_Start(object sender, EventArgs args)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ObjectFactory.Initialize();
            ObjectFactory.Configure(Fr8.Infrastructure.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            ObjectFactory.Configure(PlanDirectoryBootStrapper.LiveConfiguration);

            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            Fr8.Infrastructure.Utilities.Server.ServerPhysicalPath = Server.MapPath("~");
            var segmentWriteKey = Fr8.Infrastructure.Utilities.Configuration.CloudConfigurationManager.GetSetting("SegmentWriteKey");
            if (!string.IsNullOrEmpty(segmentWriteKey))
                Analytics.Initialize(segmentWriteKey);
            await ObjectFactory.GetInstance<ISearchProvider>().Initialize(false);
            await GenerateManifestPages();
        }

        private async Task GenerateManifestPages()
        {
            var systemUser = ObjectFactory.GetInstance<Fr8Account>().GetSystemUser()?.EmailAddress?.Address;
            var generator = ObjectFactory.GetInstance<IManifestPageGenerator>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWorkFactory>().Create())
            {
                var generateTasks = new List<Task>();
                foreach (var manifestName in uow.MultiTenantObjectRepository.Query<ManifestDescriptionCM>(systemUser, x => true).Select(x => x.Name).Distinct())
                {
                    generateTasks.Add(generator.Generate(manifestName, GenerateMode.GenerateIfNotExists));
                }
                await Task.WhenAll(generateTasks);
            }
        }
    }
}