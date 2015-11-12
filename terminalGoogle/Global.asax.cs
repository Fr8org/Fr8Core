using System;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;

namespace terminalGoogle
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
        }
    }
}