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
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE);
            GlobalConfiguration.Configure(RoutesConfig.Register);
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            // StructureMap Dependencies configuration
            
            TerminalSendGridStructureMapBootstrapper.ConfigureDependencies(TerminalSendGridStructureMapBootstrapper.DependencyType.LIVE);
            TerminalBootstrapper.ConfigureLive();
        }
    }
}
