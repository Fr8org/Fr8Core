using Fr8Infrastructure.StructureMap;
using System;
using System.Web.Http;
using TerminalBase.Infrastructure;
using DependencyType = Fr8Infrastructure.StructureMap.StructureMapBootStrapper.DependencyType;
namespace terminalGoogle
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            TerminalBootstrapper.ConfigureLive();
            StructureMapBootStrapper.ConfigureDependencies(DependencyType.LIVE).ConfigureGoogleDependencies(DependencyType.LIVE);
        }
    }
}