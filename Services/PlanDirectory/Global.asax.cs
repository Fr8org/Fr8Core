using System;
using System.Web.Http;
using System.Web.Routing;
using Segment;
using StructureMap;
using Data.Infrastructure.AutoMapper;
using PlanDirectory.App_Start;
using PlanDirectory.Infrastructure;

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
            var segmentWriteKey = new ConfigRepository().Get("SegmentWriteKey");
            if (!string.IsNullOrEmpty(segmentWriteKey))
                Analytics.Initialize(segmentWriteKey);

            await ObjectFactory.GetInstance<ISearchProvider>().Initialize(false);
        }
    }
}