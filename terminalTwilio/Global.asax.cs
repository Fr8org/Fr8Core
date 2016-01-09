using System.Web;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using TerminalBase.Infrastructure;

namespace terminalTwilio
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.LIVE;

            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).ConfigureTwilioDependencies(dependencyType);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
