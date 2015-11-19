using System;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;
namespace terminalGoogle
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE).ConfigureGoogleDependencies(DependencyType.LIVE);
        }
    }
}