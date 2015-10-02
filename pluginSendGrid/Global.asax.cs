using Data.Infrastructure.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace pluginSendGrid
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            Core.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Core.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
        }
    }
}
