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
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            // StructureMap Dependencies configuration
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            TerminalSendGridStructureMapBootstrapper.ConfigureDependencies(TerminalSendGridStructureMapBootstrapper.DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
