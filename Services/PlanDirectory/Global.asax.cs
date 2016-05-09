using System;
using System.Web.Routing;
using StructureMap;
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
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ObjectFactory.Initialize();
            ObjectFactory.Configure(StructureMapBootStrapper.LiveConfiguration);
            ObjectFactory.Configure(PlanDirectoryBootStrapper.LiveConfiguration);

            DataAutoMapperBootStrapper.ConfigureAutoMapper();
        }
    }
}