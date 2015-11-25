using System.Web;
using System.Web.Http;
using Data.Infrastructure.AutoMapper;
using Hub.StructureMap;
using TerminalBase.Infrastructure;

namespace terminalSendGrid
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            const StructureMapBootStrapper.DependencyType dependencyType = StructureMapBootStrapper.DependencyType.LIVE;

            GlobalConfiguration.Configure(WebApiConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(dependencyType).SendGridConfigureDependencies(dependencyType);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
