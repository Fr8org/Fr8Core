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
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).ConfigureTwilioDependencies(dependencyType);
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();
            // StructureMap Dependencies configuration
            
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
