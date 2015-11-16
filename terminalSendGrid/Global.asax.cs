using Data.Infrastructure.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Hub.StructureMap;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace terminalSendGrid
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            Hub.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Hub.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
        }
    }
}
