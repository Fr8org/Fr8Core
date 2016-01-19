using System;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using TerminalBase.Infrastructure;
using DependencyType = Hub.StructureMap.StructureMapBootStrapper.DependencyType;
namespace terminalGoogle
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            TerminalBootstrapper.ConfigureLive();
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE).ConfigureGoogleDependencies(DependencyType.LIVE);
        }
    }
}