using System;
using System.Web.Http;
using System.Web.Routing;
using Segment;
using StructureMap;
using Utilities;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using PlanDirectory.App_Start;
using PlanDirectory.Infrastructure;

namespace PlanDirectory
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs args)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ObjectFactory.Initialize();
            ObjectFactory.Configure(StructureMapBootStrapper.LiveConfiguration);
            ObjectFactory.Configure(PlanDirectoryBootStrapper.LiveConfiguration);

            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            Utilities.Server.ServerPhysicalPath = Server.MapPath("~");
            var segmentWriteKey = new ConfigRepository().Get("SegmentWriteKey");
            Analytics.Initialize(segmentWriteKey);
        }
    }
}